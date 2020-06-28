import * as katex from 'katex/dist/blatex.js';

console.log(katex);
console.log(Object.keys(katex));
var arg0 = "c";
var result = katex.renderToString(arg0);

console.log(JSON.stringify(result));

if (typeof globalThis !== 'undefined') {
    globalThis.katex = katex;
}
export default katex;
export const blatex = katex;