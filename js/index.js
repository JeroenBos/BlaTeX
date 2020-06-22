export const katex = require('katex');

if (typeof globalThis !== 'undefined') {
    globalThis.katex = katex;
}
