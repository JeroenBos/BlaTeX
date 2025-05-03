import { Page } from 'playwright'
import forwardBrowserDevConsole from './forwardBrowserDevConsole'
import path, { resolve } from 'path'
import fs from 'fs'
import { PNG } from 'pngjs'

export function registerDefaultHooks(
    test: typeof import('playwright/test').test,
    options: {
        throwOnError?: boolean
        timeout?: number
        skip?: boolean
    } = {}
) {
    if (options.skip) {
        return test
    }
    options.throwOnError ??= false

    type CustomAutoFixtures = {
        forwardBrowserDevConsole: void
    }

    test = test.extend<CustomAutoFixtures>({
        forwardBrowserDevConsole: [forwardBrowserDevConsole(options), { auto: true }],
    })

    test.beforeEach(async ({ page }) => {
        await page.setViewportSize({ width: 1000, height: 500 })
        page.setDefaultNavigationTimeout(10_000)
        page.setDefaultTimeout(10_000)
        test.setTimeout(options.timeout ?? 30_000)
    })
    test.afterEach(async ({ page }, testInfo) => {
        if (testInfo.expectedStatus === 'skipped') {
            return
        }

        if (testInfo.status !== testInfo.expectedStatus) {
            // unfortunately when a test times out, Playwright disposes of the page, so we can't screenshot anymore
            const testIdentifier = testInfo.title + (testInfo.project.retries === 0 ? '' : `#${testInfo.retry + 1}`)
            await takeScreenshot(page, testIdentifier, { save: true })
        }
    })
    return test
}
export async function takeScreenshot(page: Page, testIdentifier: string, { save }: { save?: boolean } = { save: process.env.CI === "true" }): Promise<Buffer> {
    console.debug(`Screenshotting '${testIdentifier}'..`)

    const screenshot = await page.screenshot({ fullPage: true })

    if (save) {
        const fileName = resolve(path.join('test-results', 'actual_snapshots', toValidPath(testIdentifier) + '.png'))
        fs.mkdirSync(path.dirname(fileName), { recursive: true })
        fs.writeFileSync(fileName, PNG.sync.write(PNG.sync.read(screenshot)) as unknown as NodeJS.ArrayBufferView)
        console.debug('Saved screenshot')
    }
    return screenshot
}
function toValidPath(s: string): string {
    return s.replaceAll('?', '_').replaceAll('/', '_').replaceAll('<', '_').replaceAll('>', '_').replaceAll(':', '_')
}
