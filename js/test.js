var katex = require('/home/jeroen/git/JeroenAlpha/BlaTeX/wwwroot/js/blatex_wrapper.js');

console.log(katex);
console.log(Object.keys(katex));

katex = katex.default;
console.log("katex = katex.default;");
console.log(katex);
console.log(Object.keys(katex));



var arg0 = "c";
var result = katex.renderToString(arg0);

console.log(JSON.stringify(result));