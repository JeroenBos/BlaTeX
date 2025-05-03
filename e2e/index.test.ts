import { test as base, expect } from 'playwright/test'
import { registerDefaultHooks, takeScreenshot } from './fixtures/registration'

const test = registerDefaultHooks(base)

test("index.html", async ({ page }) => {
    const testIdentifier = "index"
    await page.goto('http://localhost:8080')

    await page.waitForSelector('[data-test="loaded"]')

    const screenshot = await takeScreenshot(page, testIdentifier)
    expect(screenshot).toMatchSnapshot(testIdentifier + '.png', { maxDiffPixels: 0 })
})
