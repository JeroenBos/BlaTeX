import type { FullConfig, FullResult, Reporter, Suite, TestCase, TestError, TestResult, TestStep } from '@playwright/test/reporter'
import chalk from 'chalk'
import uniqBy from 'lodash/uniqBy'

type Event =
    | {
          event: 'stepStart' | 'stepEnd'
          timestamp: number
          step: TestStep
      }
    | {
          event: 'stdout' | 'stderr'
          timestamp: number
          message: string | Buffer
      }
    | {
          event: 'error'
          timestamp: number
          error: TestError
      }
    | {
          event: 'placeholder'
          timestamp: number
      }
    | {
          event: 'begin'
          timestamp: number
          test: TestCase
      }

function chalkLine(text: string, linePredicate: (line: string) => boolean, chalk: (s: string) => string) {
    return text
        .split('\n')
        .map(line => (linePredicate(line) ? chalk(line) : line))
        .join('\n')
}
function formatError(error: TestError): string {
    const indentation = '      '
    let message = error.message ?? (error.value ? JSON.stringify(error.value) : error.snippet ?? "An error occurred (that's all folks)")

    if (message.includes('Screenshot comparison failed')) {
        const endIndex = message.indexOf('Expected:')
        message = message.substring('Error: '.length, endIndex === -1 ? undefined : endIndex)
    }
    message = message.replaceAll('Call log:', '')
    message = chalkLine(message, line => line.includes('TimeoutError:'), chalk.red)

    return message.replaceAll('\n\n', '\n').replaceAll('\n', `\n${indentation}`).trim() + formatLocation(error)
}

function formatTimestamp(begin: Event & { event: 'begin' }, event: Event) {
    const duration_s = (event.timestamp - begin.timestamp) / 1000
    const timestamp = `${duration_s < 10 ? ' ' : ''}${duration_s.toFixed(3)}`
    return timestamp
}
function formatSeconds(duration_ms: number): string {
    return (duration_ms / 1000).toFixed(1) + 's'
}
function formatLocation(error: TestError) {
    if (error.snippet) {
        return `\n${error.snippet}`
    }
    return ''
}

function getTestTitle(test: TestCase): string {
    const title = test
        .titlePath()
        .filter(s => s !== 'Screenshot tests' && s !== 'default')
        .filter(Boolean)
        .join(' - ')
        .replaceAll('\n', '')
    if (title.startsWith('screenshots')) {
        return title.substring('screenshots/'.length)
    }
    if (title.startsWith('pages')) {
        return title.substring('pages/'.length)
    }
    return title
}

class CustomReporter implements Reporter {
    private events: Record<string /* =test.id */, Event[]> = {}
    private statusCounts = { passed: 0, failed: 0, timedOut: 0, skipped: 0, interrupted: 0 }
    private flakyTests: TestCase[] = []

    onBegin(config: FullConfig, suite: Suite) {
        console.log(
            `Running ${suite.allTests().length} test${suite.allTests().length === 1 ? '' : 's'} using ${config.workers} worker${config.workers === 1 ? '' : 's'}`
        )
        console.log()
    }

    onStepBegin(test: TestCase, _result: TestResult, step: TestStep): void {
        const titleOfFirstStepAfterActualTest = 'After Hooks'
        if (step.title === titleOfFirstStepAfterActualTest) {
            // insert a placeholder, see this.weaveError
            this.events[test.id].push({ event: 'placeholder', timestamp: Date.now() })
        }
        this.events[test.id].push({ event: 'stepStart', step, timestamp: Date.now() })
    }
    onStepEnd(test: TestCase, _result: TestResult, step: TestStep): void {
        const error = step.error
        if (error) {
            const errorAlreadyRegisteredOnSubStep = step.steps.some(substep => error.snippet && error.snippet === substep.error?.snippet)
            if (!errorAlreadyRegisteredOnSubStep) {
                this.events[test.id].push({ event: 'error', error, timestamp: Date.now() })
            }
        }
        this.events[test.id].push({ event: 'stepEnd', step, timestamp: Date.now() })
    }

    onTestBegin(test: TestCase, _result: TestResult) {
        this.events[test.id] = [{ event: 'begin', timestamp: Date.now(), test }]
    }

    onTestEnd(test: TestCase, result: TestResult) {
        if (result.error) {
            this.weaveError(test, result)
        }
        if (test.retries === result.retry || ['passed', 'interrupted', 'skipped'].includes(result.status)) {
            this.statusCounts[result.status]++
        }
        if (test.outcome() === 'flaky') {
            this.flakyTests.push(test)
        }
        const events = this.events[test.id].splice(0)
        if (events.length === 0) {
            throw new Error('CustomReporter error: no events found')
        }
        if (events[0].event !== 'begin') {
            throw new Error('CustomReporter error: first event was not "begin"')
        }
        const beginEventCount = events.filter(({ event }) => event === 'begin').length
        if (beginEventCount !== 1) {
            throw new Error(`CustomReporter error: multiple begin events found (${beginEventCount})`)
        }

        if (result.status === 'failed' || result.status === 'timedOut' || process.env.E2E_TRACE === 'true') {
            for (const event of events.sort((a, b) => a.timestamp - b.timestamp)) {
                const timestamp = formatTimestamp(events[0], event)
                switch (event.event) {
                    case 'begin':
                        const duration = formatSeconds(result.duration)
                        function getSummary(): string {
                            if (result.status === 'failed')
                                return result.retry === test.retries ? chalk.bgRed('FAIL') + '  ' : chalk.bgYellow(`FAIL#${result.retry + 1}`)
                            else if (result.status === 'timedOut')
                                return result.retry === test.retries ? chalk.bgRed('TIME') + '  ' : chalk.bgYellow(`TIME#${result.retry + 1}`)
                            return result.retry === 0 ? chalk.bgGreen('PASS#1') : chalk.bgGreen('PASS') + chalk.bgYellow(`#${result.retry + 1}`)
                        }
                        console.log(` ${getSummary()} ${getTestTitle(event.test)} (${result.status === 'timedOut' ? chalk.red(duration) : duration})`)
                        break
                    case 'stdout':
                    case 'stderr':
                        const message = typeof event.message === 'string' ? event.message : event.message.toString()
                        console.log(`${timestamp}: ${message.trimEnd()}`)
                        break
                    case 'error':
                        console.log(`${timestamp}: ${formatError(event.error)}`)
                        break
                    case 'stepStart':
                    case 'stepEnd':
                        break // we may log these later
                    case 'placeholder':
                        break // if we encounter the placeholder, then there was no error, so don't do anything
                    default:
                        throw new Error(`Encountered unexpected '${(event satisfies never as { event: string }).event}' event`)
                }
            }
            if (result.status === 'timedOut') {
                console.log(`        ${chalk.red('Timed out')}`)
            }
            console.log()
        } else if (result.status === 'skipped') {
            console.log(` ${chalk.gray('SKIP')}   ${getTestTitle(events[0].test)}`)
        } else if (result.status !== 'interrupted') {
            const retryWarning = result.retry === 0 ? '  ' : chalk.bgYellow(`#${result.retry + 1}`)
            console.log(` ${chalk.bgGreen('PASS')}${retryWarning} ${getTestTitle(events[0].test)} (${formatSeconds(result.duration)})`)
        }
    }

    onEnd(result: FullResult) {
        if (this.statusCounts['interrupted'] !== 0 && process.env.CI) {
            console.log(chalk.red('Interrupted..'))
            return
        }

        const totalTestCount = Object.values(this.statusCounts).reduce((a, b) => a + b)
        const failedCount = this.statusCounts['failed'] + this.statusCounts['timedOut']

        if (failedCount !== 0) {
            console.log(`${chalk.red(`${failedCount} out of ${totalTestCount} tests failed`)} (${formatSeconds(result.duration)})`)

            if (this.statusCounts['timedOut'] !== 0) {
                console.log(chalk.yellow(`- ${this.statusCounts['timedOut']} tests timed out`))
            }
        }
        if (this.statusCounts['passed'] !== 0) {
            console.log(`${chalk.green(`${this.statusCounts['passed']} tests passed`)} (${formatSeconds(result.duration)})`)
        }

        if (this.flakyTests.length !== 0) {
            console.log(chalk.yellow(`${this.flakyTests.length} ${this.flakyTests.length === 1 ? 'was' : 'were'} flaky`))
            for (const flakyTest of uniqBy(this.flakyTests, test => test.title).sort()) {
                console.log(`::warning file=${flakyTest.location.file},line=${flakyTest.location.line},title="Flaky test"::${flakyTest.title}`)
            }
        }
        if (this.statusCounts['skipped'] !== 0) {
            console.log(chalk.gray(`${this.statusCounts['skipped']} tests skipped`))
        }
    }

    private weaveError(test: TestCase, result: TestResult) {
        // Errors can be thrown at various places: for example in hooks, or browser-interacting steps.
        // Those errors are seen by this reporter in the `onStepEnd`.
        // However if an error is thrown in the actual test itself (i.e. the non-hook part), we don't know about that error until `onTestEnd`
        // That's annoying because then we can't display the error in chronological order.
        // Our workaround is that we place a placeholder at just after the end of the test (excluding hooks), which is where the error would have been
        // Then here, at `onTestEnd` we insert the error that we haven't encountered yet if any (which must have happened during the actual test)
        // and insert it at the placeholder, ensuring our logs are chronological.
        // A further remark: we're comparing errors for equality by JSON because they are unfortunately not reference-equal
        const knownErrors = this.events[test.id]
            .filter(event => event.event === 'error')
            .map(event => {
                if ('error' in event) return JSON.stringify(event.error)
                return null
            })
        const errorsToInsert = result.errors.filter(error => !knownErrors.includes(JSON.stringify(error)))
        const placeholderIndex = this.events[test.id].findIndex(event => event.event === 'placeholder')

        if (placeholderIndex !== -1) {
            const placeholder = this.events[test.id][placeholderIndex]
            this.events[test.id].splice(
                placeholderIndex,
                1,
                ...errorsToInsert.map<Event>(error => ({ timestamp: placeholder.timestamp, event: 'error', error: error }))
            )
        } else {
            this.events[test.id].push(...errorsToInsert.map<Event>(error => ({ error, event: 'error', timestamp: Date.now() })))
        }
    }

    private onStdOrErrOut(event: 'stderr' | 'stdout', chunk: string | Buffer, test?: TestCase, _result?: TestResult) {
        if (test === undefined) {
            console.log(typeof chunk === 'string' ? chunk : chunk.toString())
        } else {
            this.events[test.id].push({ event, message: chunk, timestamp: Date.now() })
        }
    }
    onStdOut(chunk: string | Buffer, test?: TestCase, result?: TestResult): void {
        this.onStdOrErrOut('stdout', chunk, test, result)
    }
    onStdErr(chunk: string | Buffer, test?: TestCase, result?: TestResult): void {
        this.onStdOrErrOut('stderr', chunk, test, result)
    }

    onError(error: TestError): void {
        console.log('Global error:')
        console.log(formatError(error))
    }
    printsToStdio(): boolean {
        return true // prevents the default DotReporter from being added
    }
}

export default CustomReporter
