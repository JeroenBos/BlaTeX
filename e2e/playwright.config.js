import { defineConfig } from '@playwright/test'

export default defineConfig({
    use: {
        baseURL: 'https://client.hero.deepinsight.internal',
        ignoreHTTPSErrors: true,
        browserName: 'chromium',
        trace: process.env.CI ? 'on' : 'off',
    },
    snapshotPathTemplate: '{testDir}/__artifacts__/{arg}{ext}',
    reporter: './CustomReporter.ts',
    workers: 8,
    fullyParallel: true,
    reportSlowTests: {
        threshold: 60_000,
        max: 0, // means all
    },
    expect: {
        toHaveScreenshot: {
            threshold: 0,
            maxDiffPixels: 0,
        },
        toMatchSnapshot: {
            threshold: 0,
            maxDiffPixels: 0,
        },
        timeout: 10_000,
    },
})
