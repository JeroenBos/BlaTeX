import { PlaywrightTestArgs, PlaywrightTestOptions, TestFixture } from '@playwright/test'
import { ConsoleMessage, Page } from 'playwright'

export interface LoggedInFlag {
    loggedIn?: boolean
}

function forwardBrowserDevConsole(page: Page) {
    const tracker = {
        errorCount: 0,
        warningCount: 0,
        logCount: 0,
        infoCount: 0,
        debugCount: 0,
        get totalCount(): number {
            return this.debugCount + this.infoCount + this.logCount + this.warningCount + this.errorCount
        },
        get hasErrors(): boolean {
            return this.errorCount !== 0
        },
        get hasWarnings(): boolean {
            return this.warningCount !== 0
        },
        throwIfHasErrors() {
            if (this.hasErrors) {
                throw new Error('Dev console has errors')
            }
        },
    }

    function pageErrorHandler($: Error) {
        console.error(`[browser] ${$.message}`)
        tracker.errorCount++
    }
    function consoleHandler(event: ConsoleMessage) {
        const text = trimRedundantStackTrace(toText(event))

        switch (event.type()) {
            case 'debug':
                console.debug(`[browser] ${text}`)
                tracker.debugCount++
                break
            case 'log':
            case 'info':
                console.log(`[browser] ${text}`)
                tracker.logCount++
                break
            case 'warn':
            case 'warning':
                console.warn(`[browser] ${text}`)
                tracker.warningCount++
                break
            case 'error':
                console.error(`[browser] ${text}`)
                tracker.errorCount++
                break
        }
    }
    page.addListener('pageerror', pageErrorHandler)
    page.addListener('console', consoleHandler)
    return tracker
}

function toText(event: { text: () => string }): string {
    const text = event.text()
    if (text === 'JSHandle@map') {
        return '{ Map(?) { ... elements ... } }'
    }
    return text
}

function trimRedundantStackTrace(text: string): string {
    const lines = text.split('\n')
    const indexOfLastUsefulLine = lines.findIndex(line => line.startsWith('at pages/forwardBrowserDevConsole.ts'))
    if (indexOfLastUsefulLine === -1) {
        return text
    }
    return lines.slice(0, indexOfLastUsefulLine + 1).join(',')
}

type Options = {
    throwOnError?: boolean
}

function createFixture(options: Options): TestFixture<void, PlaywrightTestArgs & PlaywrightTestOptions> {
    return async ({ page }, use, testInfo) => {
        const devConsole = forwardBrowserDevConsole(page)

        await use()

        // only log on success, to not detract attention from the real error
        if (testInfo.status === 'passed' && (options.throwOnError ?? true)) {
            devConsole.throwIfHasErrors()
        }
    }
}

export default createFixture
