import * as katex from './blatex/dist/blatex';

console.log(katex);
console.log(Object.keys(katex));
var arg0 = "c";
var result = katex.renderToString(arg0);

console.log(JSON.stringify(result));

if (typeof globalThis !== 'undefined') {
    globalThis.katex = katex;
}
// export default katex;
exports.default = katex;