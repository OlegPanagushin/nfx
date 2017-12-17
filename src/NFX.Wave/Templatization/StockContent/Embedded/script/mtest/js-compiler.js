function hello(text) {
    alert(WAVE.strDefault(text, "Hello"));
}

function alt() {
  alert();
}

function render(root, ctx){
var Ør = arguments[0];
if (WAVE.isString(Ør))
  Ør = WAVE.id(Ør);
var Ø1 = WAVE.ce('div');
Ø1.addEventListener('click', function() { console.log('kaka'); }, false);
var Ø2 = WAVE.ce('div');
Ø2.setAttribute('class', 'title');
Ø2.setAttribute('data-alert', 'alert(\x27just data\x27)');
Ø2.setAttribute('data-alert-script', '\x3Cscript\x3Ealert(\x22script\x22);\x3C\x2Fscript\x3E');
Ø1.appendChild(Ø2);
var Ø3 = WAVE.ce('div');
Ø3.setAttribute('id', 'rate');
Ø1.appendChild(Ø3);
var Ø4 = WAVE.ce('div');
Ø4.setAttribute('class', '@color@');
Ø1.appendChild(Ø4);
var Ø5 = WAVE.ce('div');
Ø5.setAttribute('class', 'stub @color@');
Ø1.appendChild(Ø5);
var Ø6 = WAVE.ce('div');
Ø6.innerText = '\x3Cscript\x3Ealert(\x22\x27\x3Cscript\x3Ealert(\x27@color@\x27);\x3C\x2Fscript\x3E\x27text\x22)\x3C\x2Fscript\x3E';
Ø1.appendChild(Ø6);
var Ø7 = WAVE.ce('div');
var Ø8 = WAVE.ce('div');
Ø8.setAttribute('class', '@color@');
var Ø9 = WAVE.ce('div');
var Ø10 = WAVE.ce('div');
Ø10.setAttribute('class', '@color@');
var Ø11 = WAVE.ce('div');
var Ø12 = WAVE.ce('div');
Ø12.setAttribute('class', '@color@');
var Ø13 = WAVE.ce('div');
var Ø14 = WAVE.ce('div');
Ø14.setAttribute('class', '@color@');
var Ø15 = WAVE.ce('div');
Ø15.innerText = ',.\x2F[\x5C]{}|!@#$%^\x26*()_+=-~`\x27';
Ø14.appendChild(Ø15);
Ø13.appendChild(Ø14);
Ø12.appendChild(Ø13);
Ø11.appendChild(Ø12);
Ø10.appendChild(Ø11);
Ø9.appendChild(Ø10);
Ø8.appendChild(Ø9);
Ø7.appendChild(Ø8);
Ø1.appendChild(Ø7);
var Ø16 = WAVE.ce('div');
Ø16.setAttribute('data-height', '@height@');
Ø16.setAttribute('class', '@color@');
Ø1.appendChild(Ø16);
var Ø17 = WAVE.ce('div');
Ø17.setAttribute('id', 'controls');
var Ø18 = WAVE.ce('input');
Ø18.setAttribute('value', '2013-06-06');
Ø18.setAttribute('type', 'date');
Ø17.appendChild(Ø18);
var Ø19 = WAVE.ce('input');
Ø19.setAttribute('value', '234.11');
Ø19.setAttribute('type', 'text');
Ø17.appendChild(Ø19);
var Ø20 = WAVE.ce('input');
Ø20.setAttribute('value', '234.11');
Ø20.setAttribute('type', 'number');
Ø17.appendChild(Ø20);
Ø1.appendChild(Ø17);
var Ø21 = WAVE.ce('div');
Ø21.setAttribute('id', 'container');
var Ø22 = WAVE.ce('h1');
Ø22.innerText = 'Animation Test';
Ø21.appendChild(Ø22);
var Ø23 = WAVE.ce('button');
Ø23.innerText = 'Highlight';
Ø23.setAttribute('class', 'highlight');
Ø23.addEventListener('click', function() { hello('highlight'); }, false);
Ø21.appendChild(Ø23);
var Ø24 = WAVE.ce('button');
Ø24.innerText = 'Fade';
Ø24.setAttribute('class', 'fade');
Ø24.addEventListener('click', function() { hello('fade'); }, false);
Ø21.appendChild(Ø24);
var Ø25 = WAVE.ce('button');
Ø25.innerText = 'Rizzle';
Ø25.setAttribute('class', 'rizzle');
Ø25.addEventListener('click', hello, false);
Ø21.appendChild(Ø25);
var Ø26 = WAVE.ce('button');
Ø26.innerText = 'Knit';
Ø26.setAttribute('class', 'knit');
Ø26.addEventListener('click', hello, false);
Ø21.appendChild(Ø26);
var Ø27 = WAVE.ce('button');
Ø27.innerText = 'Shrink';
Ø27.setAttribute('class', 'shrink');
Ø27.addEventListener('click', hello, false);
Ø21.appendChild(Ø27);
var Ø28 = WAVE.ce('button');
Ø28.innerText = 'Rotate';
Ø28.setAttribute('class', 'rotate');
Ø28.addEventListener('click', hello, false);
Ø21.appendChild(Ø28);
var Ø29 = WAVE.ce('button');
Ø29.innerText = 'Boom';
Ø29.setAttribute('class', 'boom');
Ø29.addEventListener('click', hello, false);
Ø21.appendChild(Ø29);
var Ø30 = WAVE.ce('button');
Ø30.innerText = 'Squeeze';
Ø30.setAttribute('class', 'squeeze');
Ø30.addEventListener('click', hello, false);
Ø21.appendChild(Ø30);
var Ø31 = WAVE.ce('button');
Ø31.innerText = 'Deform';
Ø31.setAttribute('class', 'deform');
Ø31.addEventListener('click', hello, false);
Ø21.appendChild(Ø31);
Ø1.appendChild(Ø21);
var Ø32 = WAVE.ce('h1');
Ø32.innerText = 'Compiler output example';
Ø1.appendChild(Ø32);
var Ø33 = WAVE.ce('code');
Ø33.innerText = '\x0D\x0A      function noRoot() {\x0D\x0A        var ljs_useCtx = WAVE.isObject(ctx);\x0D\x0A        var ljs_1 = document.createElement(\x27section\x27);\x0D\x0A        var ljs_2 = \x27sect\x27;\x0D\x0A        ljs_1.setAttribute(\x27id\x27, ljs_useCtx ? WAVE.strHTMLTemplate(ljs_2, ctx) : WAVE.strEscapeHTML(ljs_2));\x0D\x0A        var ljs_3 = \x27sect\x27;\x0D\x0A        ljs_1.setAttribute(\x27class\x27, ljs_useCtx ? WAVE.strHTMLTemplate(ljs_3, ctx) : WAVE.strEscapeHTML(ljs_3));\x0D\x0A        var ljs_4 = \x27 Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cum sollicitudin interdum,      sollicitudin condimentum montes nulla bibendum aliquam velit? Fermentum mattis aenean nec...      Orci proin litora nec ullamcorper?    \x27;\x0D\x0A        ljs_1.innerText = ljs_useCtx ? WAVE.strHTMLTemplate(ljs_4, ctx) : WAVE.strEscapeHTML(ljs_4);\x0D\x0A        if (typeof(root) !== \x27undefined\x27 \x26\x26 root !== null) {\x0D\x0A        if (WAVE.isString(root))\x0D\x0A        root = WAVE.id(root);\x0D\x0A        if (WAVE.isObject(root))\x0D\x0A        root.appendChild(ljs_1);\x0D\x0A      }\x0D\x0A    ';
Ø1.appendChild(Ø33);
if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
return Ø1;
}


function noRoot() {
var Ør = arguments[0];
if (WAVE.isString(Ør))
  Ør = WAVE.id(Ør);
var Ø1 = WAVE.ce('section');
Ø1.innerText = '\x0D\x0A      Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cum sollicitudin interdum,\x0D\x0A      sollicitudin condimentum montes nulla bibendum aliquam velit? Fermentum mattis aenean nec...\x0D\x0A      Orci proin litora nec ullamcorper?\x0D\x0A    ';
Ø1.setAttribute('id', 'sect');
Ø1.setAttribute('class', 'sect');
if (WAVE.isObject(Ør)) Ør.appendChild(Ø1);
return Ø1;
}