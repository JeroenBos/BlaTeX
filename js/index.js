import * as katex from './blatex/dist/blatex';

if (typeof globalThis !== 'undefined') {
    globalThis.katex = katex;
}

exports.default = katex;