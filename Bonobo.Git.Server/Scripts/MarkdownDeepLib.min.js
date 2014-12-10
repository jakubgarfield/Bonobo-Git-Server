// MarkdownDeep - http://www.toptensoftware.com/markdowndeep
// Copyright (C) 2010-2011 Topten Software
// Minified by MiniME from toptensoftware.com
var MarkdownDeep=new(function(){function S(b,e){if(b.indexOf!==undefined)return b.indexOf(e);for(var c=0;c<b.length;c++)
if(b[c]===e)return c;return-1}function i(){this.bz=new E(this);this.bC=[];this.bE=new F();this.bF=new F()}i.prototype={
SafeMode:false,ExtraMode:false,MarkdownInHtml:false,AutoHeadingIDs:false,UrlBaseLocation:null,UrlRootLocation:null,
NewWindowForExternalLinks:false,NewWindowForLocalLinks:false,NoFollowLinks:false,HtmlClassFootnotes:"footnotes",
HtmlClassTitledImages:null,RenderingTitledImage:false,FormatCodeBlockAttributes:null,FormatCodeBlock:null,
ExtractHeadBlocks:false,HeadBlockContent:""};var a=i.prototype;function ao(b,c,e,g){return b.slice(0,c).concat(g).concat
(b.slice(c+e))}i.prototype.GetListItems=function(k,n){var c=this.aE(k),b;for(b=0;b<c.length;b++){var e=c[b];if((e.v==23
||e.v==15||e.v==22)&&e.C){c=ao(c,b,1,e.C);b--;continue}if(n<e.ay)break}b--;if(b<0)return null;var h=c[b];if(h.v!=21&&h.v
!=20)return null;var g=[],m=h.C;for(var j=0;j<m.length;j++)g.push(m[j].ay);b++;if(b<c.length)g.push(c[b].ay);else g.push
(k.length);return g};i.prototype.Transform=function(c){var n=c.indexOf("\r");if(n>=0){var m=c.indexOf("\n");if(m>=0)if(m
<n)c=c.replace(/\n\r/g,"\n");else c=c.replace(/\r\n/g,"\n");c=c.replace(/\r/g,"\n")}this.HeadBlockContent="";var k=this.
aE(c);if(this.bn!=null){var j=[];for(var r in this.bn)j.push(this.bn[r]);j.sort(function(p,q){return q.Abbr.length-p.
Abbr.length});this.bn=j}var b=this.bF;b.K();for(var g=0;g<k.length;g++){var s=k[g];s.l(this,b)}if(this.bI.length>0){b.x(
'\n<div class="');b.x(this.HtmlClassFootnotes);b.x('">\n');b.x("<hr />\n");b.x("<ol>\n");for(var g=0;g<this.bI.length;
g++){var h=this.bI[g];b.x('<li id="fn:');b.x(h.X);b.x('">\n');var o='<a href="#fnref:'+h.X+
'" rev="footnote">&#8617;</a>',e=h.C[h.C.length-1];if(e.v==12){e.v=29;e.X=o}else{e=new B();e.N=0;e.v=29;e.X=o;h.C.push(e
)}h.l(this,b);b.x("</li>\n")}b.x("</ol>\n");b.x("</div>\n")}return b.bh()};i.prototype.OnQualifyUrl=function(b){if(aj(b)
)return b;if(au(b,"/")){var e=this.UrlRootLocation;if(!e){if(!this.UrlBaseLocation)return b;var c=this.UrlBaseLocation.
indexOf("://");if(c==-1)c=0;else c+=3;c=this.UrlBaseLocation.indexOf("/",c);e=c<0?this.UrlBaseLocation:this.
UrlBaseLocation.substr(0,c)}return e+b}else{if(!this.UrlBaseLocation)return b;if(!T(this.UrlBaseLocation,"/"))
return this.UrlBaseLocation+"/"+b;else return this.UrlBaseLocation+b}};i.prototype.OnGetImageSize=function(b,c){
return null};i.prototype.OnPrepareLink=function(b){var c=b.attributes.href;if(this.NoFollowLinks)b.attributes.rel=
"nofollow";if(this.NewWindowForExternalLinks&&aj(c)||this.NewWindowForLocalLinks&&!aj(c))b.attributes.target="_blank";b.
attributes.href=this.OnQualifyUrl(c)};i.prototype.OnPrepareImage=function(b,e){var c=this.OnGetImageSize(b.attributes.
src,e);if(c!=null){b.attributes.width=c.width;b.attributes.height=c.height}b.attributes.src=this.OnQualifyUrl(b.
attributes.src)};i.prototype.GetLinkDefinition=function(c){var b=this.bv[c];if(b==undefined)return null;else return b};a
.aE=function(b){this.bv=[];this.bs=[];this.bI=[];this.bJ=[];this.bn=null;return new D(this,this.MarkdownInHtml).aH(b)};a
.A=function(b){this.bv[b.id]=b};a.z=function(b){this.bs[b.X]=b};a.Q=function(c){var b=this.bs[c];if(b!=undefined){this.
bI.push(b);delete this.bs[c];return this.bI.length-1}else return-1};a.y=function(b,c){if(this.bn==null)this.bn=[];this.
bn[b]={Abbr:b,Title:c}};a.am=function(){return this.bn};a.aC=function(j,h,g){if(!this.AutoHeadingIDs)return null;var b=
this.bz.aB(j,h,g);if(!b)b="section";var c=b,e=1;while(this.bJ[c]!=undefined){c=b+"-"+e.toString();e++}this.bJ[c]=true;
return c};a.as=function(){this.bE.K();return this.bE};function X(b){return b>="0"&&b<="9"}function af(b){return b>="0"&&
b<="9"||b>="a"&&b<="f"||b>="A"&&b<="F"}function ac(b){return b>="a"&&b<="z"||b>="A"&&b<="Z"}function R(b){return b>="a"
&&b<="z"||b>="A"&&b<="Z"||b>="0"&&b<="9"}function ad(b){return b==" "||b=="\t"||b=="\r"||b=="\n"}function ab(b){return b
==" "||b=="\t"}function Y(b){return b=="\r"||b=="\n"}function ae(b){return b=="*"||b=="_"}function U(b,c){switch(b){case
"\\":case"`":case"*":case"_":case"{":case"}":case"[":case"]":case"(":case")":case">":case"#":case"+":case"-":case".":
case"!":return true;case":":case"|":case"=":case"<":return c}return false}function as(c,b){if(c.charAt(b)!="&")return-1;
var g=b;b++;var e;if(c.charAt(b)=="#"){b++;if(c.charAt(b)=="x"||c.charAt(b)=="X"){b++;e=af}else e=X}else e=R;if(e(c.
charAt(b))){b++;while(e(c.charAt(b)))b++;if(c.charAt(b)==";"){b++;return b}}b=g;return-1}function az(c,h){var b=c.
indexOf("\\");if(b<0)return c;var g=new F(),e=0;while(b>=0){if(U(c.charAt(b+1),h)){if(b>e)g.x(c.substr(e,b-e));e=b+1}b=c
.indexOf("\\",b+1)}if(e<c.length)g.x(c.substr(e,c.length-e));return g.bh()}function ay(e){var b=0,c=e.length;while(b<c&&
ad(e.charAt(b)))b++;while(c-1>b&&ad(e.charAt(c-1)))c--;return e.substr(b,c-b)}function ah(c){var b=c.indexOf("@");if(b<0
)return false;var e=c.lastIndexOf(".");if(e<b)return false;return true}function al(b){b=b.toLowerCase();if(b.substr(0,7)
=="http://")return true;if(b.substr(0,8)=="https://")return true;if(b.substr(0,6)=="ftp://")return true;if(b.substr(0,7)
=="file://")return true;return false}function ak(c){if(!c)return false;if(!ac(c.charAt(0)))return false;for(var e=0;e<c.
length;e++){var b=c.charAt(e);if(R(b)||b=="_"||b=="-"||b==":"||b==".")continue;return false}return true}function at(c,e,
j){var b=j-1;while(b>=e&&ad(c.charAt(b)))b--;if(b<e||c.charAt(b)!="}")return null;var k=b;b--;while(b>=e&&c.charAt(b)!=
"{")b--;if(b<e||c.charAt(b+1)!="#")return null;var g=b+2,h=c.substr(g,k-g);if(!ak(h))return null;while(b>e&&ad(c.charAt(
b-1)))b--;return{id:h,end:b}}function au(c,b){return c.substr(0,b.length)==b}function T(c,b){return c.substr(-b.length)
==b}function aj(b){return b.indexOf("://")>=0||au(b,"mailto:")}function F(){this.bq=[]}a=F.prototype;a.x=function(b){if(
b)this.bq.push(b)};a.K=function(){this.bq.length=0};a.bh=function(){return this.bq.join("")};a.aw=function(c){var g=c.
length;for(var b=0;b<g;b++){var e=Math.random();if(e>0.90&&c.charAt(b)!="@")this.x(c.charAt(b));else if(e>0.45){this.x(
"&#");this.x(c.charCodeAt(b).toString());this.x(";")}else{this.x("&#x");this.x(c.charCodeAt(b).toString(16));this.x(";")
}}};a.au=function(e,g,j){var h=g+j,b=g,c;for(c=g;c<h;c++)switch(e.charAt(c)){case"&":if(c>b)this.x(e.substr(b,c-b));this
.x("&amp;");b=c+1;break;case"<":if(c>b)this.x(e.substr(b,c-b));this.x("&lt;");b=c+1;break;case">":if(c>b)this.x(e.substr
(b,c-b));this.x("&gt;");b=c+1;break;case'"':if(c>b)this.x(e.substr(b,c-b));this.x("&quot;");b=c+1;break}if(c>b)this.x(e.
substr(b,c-b))};a.bf=function(e,g,k){var j=g+k,c=g,b;for(b=g;b<j;b++)switch(e.charAt(b)){case"&":var h=as(e,b);if(h<0){
if(b>c)this.x(e.substr(c,b-c));this.x("&amp;");c=b+1}else b=h-1;break;case"<":if(b>c)this.x(e.substr(c,b-c));this.x(
"&lt;");c=b+1;break;case">":if(b>c)this.x(e.substr(c,b-c));this.x("&gt;");c=b+1;break;case'"':if(b>c)this.x(e.substr(c,b
-c));this.x("&quot;");c=b+1;break}if(b>c)this.x(e.substr(c,b-c))};a.be=function(e,g,k){var j=g+k,c=g,b;for(b=g;b<j;b++)
switch(e.charAt(b)){case"&":var h=as(e,b);if(h<0){if(b>c)this.x(e.substr(c,b-c));this.x("&amp;");c=b+1}else b=h-1;break}
if(b>c)this.x(e.substr(c,b-c))};a.av=function(e,h,k){var j=h+k,b=h,g=0,c;for(c=h;c<j;c++){switch(e.charAt(c)){case"\t":
if(c>b)this.x(e.substr(b,c-b));b=c+1;this.x(" ");g++;while(g%4!=0){this.x(" ");g++}g--;break;case"\r":case"\n":if(c>b)
this.x(e.substr(b,c-b));this.x("\n");b=c+1;continue;case"&":if(c>b)this.x(e.substr(b,c-b));this.x("&amp;");b=c+1;break;
case"<":if(c>b)this.x(e.substr(b,c-b));this.x("&lt;");b=c+1;break;case">":if(c>b)this.x(e.substr(b,c-b));this.x("&gt;");
b=c+1;break;case'"':if(c>b)this.x(e.substr(b,c-b));this.x("&quot;");b=c+1;break}g++}if(c>b)this.x(e.substr(b,c-b))};
function G(){this.aU.apply(this,arguments)}a=G.prototype;a.D=function(){return this.by==this.start};a.J=function(){
return this.by>=this.end};a.Y=function(){if(this.by>=this.end)return true;var b=this.E.charAt(this.by);return b=="\r"||b
=="\n"||b==undefined||b==""};a.aU=function(){this.E=arguments.length>0?arguments[0]:null;this.start=arguments.length>1?
arguments[1]:0;this.end=arguments.length>2?this.start+arguments[2]:this.E==null?0:this.E.length;this.by=this.start;this.
charset_offsets={}};a.H=function(){if(this.by>=this.end)return"\x00";return this.E.charAt(this.by)};a.aM=function(){
return this.E.substr(this.by)};a.ba=function(){this.by=this.end};a.a5=function(b){this.by+=b};a.bb=function(){this.by=
this.E.indexOf("\n",this.by);if(this.by<0)this.by=this.end};a.aZ=function(){var b=this.by;if(this.E.charAt(this.by)==
"\r")this.by++;if(this.E.charAt(this.by)=="\n")this.by++;return this.by!=b};a.bc=function(){this.bb();this.aZ()};a.F=
function(b){if(this.by+b>=this.end)return"\x00";return this.E.charAt(this.by+b)};a.aW=function(b){if(this.E.charAt(this.
by)==b){this.by++;return true}return false};a.a9=function(b){if(this.E.substr(this.by,b.length)==b){this.by+=b.length;
return true}return false};a.bd=function(){var c=this.by;while(true){var b=this.E.charAt(this.by);if(b!=" "&&b!="\t"&&b!=
"\r"&&b!="\n")break;this.by++}return this.by!=c};a.a8=function(){var c=this.by;while(true){var b=this.E.charAt(this.by);
if(b!=" "&&b!="\t")break;this.by++}return this.by!=c};a.aa=function(c){c.lastIndex=this.by;var b=c.exec(this.E);if(b==
null){this.by=this.end;return false}if(b.index+b[0].length>this.end){this.by=this.end;return false}this.by=b.index;
return true};a.ad=function(g){var c=-1;for(var e in g){var b=g[e];if(b==null){b={};b.bD=-1;b.bt=-1;g[e]=b}if(b.bD==-1||
this.by<b.bD||this.by>=b.bt&&b.bt!=-1){b.bD=this.by;b.bt=this.E.indexOf(e,this.by)}if(c==-1||b.bt<c)c=b.bt}if(c==-1){c=
this.end;return false}a.by=c;return true};a.Z=function(b){this.by=this.E.indexOf(b,this.by);if(this.by<0){this.by=this.
end;return false}return true};a.az=function(){this.mark=this.by};a.W=function(){if(this.mark>=this.by)return"";else
 return this.E.substr(this.mark,this.by-this.mark)};a.a7=function(){var b=this.E.charAt(this.by);if(b>="a"&&b<="z"||b>=
"A"&&b<="Z"||b=="_"){this.by++;while(true){b=this.E.charAt(this.by);if(b>="a"&&b<="z"||b>="A"&&b<="Z"||b=="_"||b>="0"&&b
<="9")this.by++;else return true}}return false};a.a4=function(){var e=this.by;this.a8();this.az();while(true){var b=this
.H();if(R(b)||b=="-"||b=="_"||b==":"||b=="."||b==" ")this.a5(1);else break}if(this.by>this.mark){var c=ay(this.W());if(c
.length>0){this.a8();return c}}this.by=e;return null};a.a6=function(){if(this.E.charAt(this.by)!="&")return false;var b=
as(this.E,this.by);if(b<0)return false;this.by=b;return true};a.a2=function(b){if(this.E.charAt(this.by)=="\\"&&U(this.E
.charAt(this.by+1),b)){this.by+=2;return true}else{if(this.by<this.end)this.by++;return false}};function w(b){this.name=
b;this.attributes={};this.flags=0;this.closed=false;this.closing=false}a=w.prototype;a.B=function(){if(!this.attributes)
return 0;var b=0;for(var c in this.attributes)b++;return b};a.ap=function(){if(this.flags==0){this.flags=aw[this.name.
toLowerCase()];if(this.flags==undefined)this.flags=2}return this.flags};a.at=function(){var c=this.name.toLowerCase();
if(!Q[c])return false;var b=O[c];if(!b)return this.B()==0;if(!this.attributes)return true;for(var e in this.attributes)
if(!b[e.toLowerCase()])return false;if(this.attributes.href)if(!ai(this.attributes.href))return false;if(this.attributes
.src)if(!ai(this.attributes.src))return false;return true};a.aS=function(b){b.x("<");b.x(this.name);for(var c in this.
attributes){b.x(" ");b.x(c);b.x('="');b.x(this.attributes[c]);b.x('"')}if(this.closed)b.x(" />");else b.x(">")};a.aO=
function(b){b.x("</");b.x(this.name);b.x(">")};function ai(b){b=b.toLowerCase();return b.substr(0,7)=="http://"||b.
substr(0,8)=="https://"||b.substr(0,6)=="ftp://"}function ag(b){var e=b.by,c=ap(b);if(c!=null)return c;b.by=e;
return null}function ap(b){if(b.H()!="<")return null;b.a5(1);if(b.a9("!--")){b.az();if(b.Z("-->")){var g=new w("!");g.
attributes.content=b.W();g.closed=true;b.a5(3);return g}}var h=b.aW("/");b.az();if(!b.a7())return null;var c=new w(b.W()
);c.closing=h;if(h){if(b.H()!=">")return null;b.a5(1);return c}while(!b.J()){b.bd();if(b.a9("/>")){c.closed=true;
return c}if(b.aW(">"))return c;b.az();if(!b.a7())return null;var e=b.W();b.bd();if(b.aW("=")){b.bd();if(b.aW('"')){b.az(
);if(!b.Z('"'))return null;c.attributes[e]=b.W();b.a5(1)}else{b.az();while(!b.J()&&!ad(b.H())&&b.H()!=">"&&b.H()!="/")b.
a5(1);if(!b.J())c.attributes[e]=b.W()}}else c.attributes[e]=""}return null}var Q={b:1,blockquote:1,code:1,dd:1,dt:1,dl:1
,del:1,em:1,h1:1,h2:1,h3:1,h4:1,h5:1,h6:1,i:1,kbd:1,li:1,ol:1,ul:1,p:1,pre:1,s:1,sub:1,sup:1,strong:1,strike:1,img:1,a:1
},O={a:{href:1,title:1,"class":1},img:{src:1,width:1,height:1,alt:1,title:1,"class":1}},d=1,l=2,u=4,f=8,aw={p:d|f,div:d,
h1:d|f,h2:d|f,h3:d|f,h4:d|f,h5:d|f,h6:d|f,blockquote:d,pre:d,table:d,dl:d,ol:d,ul:d,form:d,fieldset:d,iframe:d,script:d|
l,noscript:d|l,math:d|l,ins:d|l,del:d|l,img:d|l,li:f,dd:f,dt:f,td:f,th:f,legend:f,address:f,hr:d|u,"!":d|u,head:d};
delete d;delete l;delete u;function C(c,e,b){this.id=c;this.url=e;if(b==undefined)this.title=null;else this.title=b}a=C.
prototype;a.aR=function(h,b,g){if(this.url.substr(0,7).toLowerCase()=="mailto:"){b.x('<a href="');b.aw(this.url);b.x('"'
);if(this.title){b.x(' title="');b.bf(this.title,0,this.title.length);b.x('"')}b.x(">");b.aw(g);b.x("</a>")}else{var e=
new w("a"),c=h.as();c.bf(this.url,0,this.url.length);e.attributes.href=c.bh();if(this.title){c.K();c.bf(this.title,0,
this.title.length);e.attributes.title=c.bh()}h.OnPrepareLink(e);e.aS(b);b.x(g);b.x("</a>")}};a.aP=function(g,h,e){var c=
new w("img"),b=g.as();b.bf(this.url,0,this.url.length);c.attributes.src=b.bh();if(e){b.K();b.bf(e,0,e.length);c.
attributes.alt=b.bh()}if(this.title){b.K();b.bf(this.title,0,this.title.length);c.attributes.title=b.bh()}c.closed=true;
g.OnPrepareImage(c,g.RenderingTitledImage);c.aS(h)};function an(b,e){var g=b.by,c=aq(b,e);if(c==null)b.by=g;return c}
function aq(b,e){b.bd();if(!b.aW("["))return null;b.az();if(!b.Z("]"))return null;var c=b.W();if(c.length==0)return null
;if(!b.a9("]:"))return null;var g=ar(b,c,e);b.a8();if(!b.Y())return null;return g}function ar(b,h,c){b.bd();if(b.Y())
return null;var e=new C(h);if(b.aW("<")){b.az();while(b.H()!=">"){if(b.J())return null;b.a2(c)}var p=b.W();if(!b.aW(">")
)return null;e.url=az(ay(p),c);b.bd()}else{b.az();var k=1;while(!b.Y()){var j=b.H();if(ad(j))break;if(h==null)if(j=="(")
k++;else if(j==")"){k--;if(k==0)break}b.a2(c)}e.url=az(ay(b.W()),c)}b.a8();if(b.H()==")")return e;var m=b.Y(),n=b.by;if(
b.Y()){b.aZ();b.a8()}var g;switch(b.H()){case"'":case'"':g=b.H();break;case"(":g=")";break;default:if(m){b.by=n;return e
}else return null}b.a5(1);b.az();while(true){if(b.Y())return null;if(b.H()==g){if(g!=")"){var o=b.by;b.a5(1);b.a8();if(h
==null&&b.H()!=")"||h!=null&&!b.Y())continue;b.by=o}break}b.a2(c)}e.title=az(b.W(),c);b.a5(1);return e}function am(b,c){
this.def=b;this.link_text=c}function ax(e,c,b){this.type=e;this.startOffset=c;this.length=b;this.X=null}function E(b){
this.bw=b;this.bB=new G();this.bG=[];this.br=false;this.bH=[]}a=E.prototype;a.ah=function(b,e,h,g){this.bj(e,h,g);if(
this.bH.length==1&&this.bw.HtmlClassTitledImages!=null&&this.bH[0].type==10){var c=this.bH[0].X;b.x('<div class="');b.x(
this.bw.HtmlClassTitledImages);b.x('">\n');this.bw.RenderingTitledImage=true;this.l(b,e);this.bw.RenderingTitledImage=
false;b.x("\n");if(c.def.title){b.x("<p>");b.bf(c.def.title,0,c.def.title.length);b.x("</p>\n")}b.x("</div>\n")}else{b.x
("<p>");this.l(b,e);b.x("</p>\n")}};a.af=function(c,b){this.ae(c,b,0,b.length)};a.ae=function(c,b,g,e){this.bj(b,g,e);
this.l(c,b)};a.ag=function(c){var b=new F();this.ae(b,c,0,c.length);return b.bh()};a.aB=function(j,n,m){this.bj(j,n,m);
var k=this.bH,c=new F();for(var h=0;h<k.length;h++){var g=k[h];switch(g.type){case 0:c.x(j.substr(g.startOffset,g.length
));break;case 9:c.x(g.X.link_text);break}this.ak(g)}var b=this.bB;b.aU(c.bh());while(!b.J()){if(ac(b.H()))break;b.a5(1)}
c.K();while(!b.J()){var e=b.H();if(R(e)||e=="_"||e=="-"||e==".")c.x(e.toLowerCase());else if(e==" ")c.x("-");else if(Y(e
)){c.x("-");b.aZ();continue}b.a5(1)}return c.bh()};a.l=function(b,h){var n=this.bH,o=n.length;for(var j=0;j<o;j++){var c
=n[j];switch(c.type){case 0:b.au(h,c.startOffset,c.length);break;case 1:b.be(h,c.startOffset,c.length);break;case 2:case
 11:case 12:case 13:b.x(h.substr(c.startOffset,c.length));break;case 8:b.x("<br />\n");break;case 3:b.x("<em>");break;
case 4:b.x("</em>");break;case 5:b.x("<strong>");break;case 6:b.x("</strong>");break;case 7:b.x("<code>");b.au(h,c.
startOffset,c.length);b.x("</code>");break;case 9:var g=c.X,m=new E(this.bw);m.br=true;g.def.aR(this.bw,b,m.ag(g.
link_text));break;case 10:var g=c.X;g.def.aP(this.bw,b,g.link_text);break;case 14:var k=c.X;b.x('<sup id="fnref:');b.x(k
.id);b.x('"><a href="#fn:');b.x(k.id);b.x('" rel="footnote">');b.x(k.index+1);b.x("</a></sup>");break;case 15:var e=c.X;
b.x("<abbr");if(e.Title){b.x(' title="');b.au(e.Title,0,e.Title.length);b.x('"')}b.x(">");b.au(e.Abbr,0,e.Abbr.length);b
.x("</abbr>");break}this.ak(c)}};a.bj=function(y,x,s){var b=this.bB;b.aU(y,x,s);var j=this.bH;j.length=0;var h=null,k=
this.bw.am(),o=k==null?/[\*\_\`\[\!\<\&\ \\]/g:null,q=this.bw.ExtraMode,g=b.by;while(!b.J()){if(o!=null&&!b.aa(o))break;
var m=b.by,c=null;switch(b.H()){case"*":case"_":c=this.P();if(c!=null)switch(c.type){case 13:case 11:case 12:if(h==null)
h=[];h.push(c);break}break;case"`":c=this.aF();break;case"[":case"!":var t=b.by;c=this.aI();if(c==null)b.by=t;break;case
"<":var e=b.by,p=ag(b);if(p!=null)if(!this.bw.SafeMode||p.at())c=this.U(1,e,b.by-e);else b.by=e;else{b.by=e;c=this.aD();
if(c==null)b.by=e}break;case"&":var e=b.by;if(b.a6())c=this.U(2,e,b.by-e);break;case" ":if(b.F(1)==" "&&Y(b.F(2))){b.a5(
2);if(!b.J()){b.aZ();c=this.U(8,m,0)}}break;case"\\":if(U(b.F(1),q)){c=this.U(0,b.by+1,1);b.a5(2)}break}if(c==null&&k!=
null&&!R(b.F(-1))){var v=b.by;for(var r in k){var n=k[r];if(b.a9(n.Abbr)&&!R(b.H())){c=this.O(15,n);break}b.bK=v}}if(c!=
null){if(m>g)j.push(this.U(0,g,m-g));j.push(c);g=b.by}else b.a5(1)}if(b.by>g)j.push(this.U(0,g,b.by-g));if(h!=null)this.
aV(j,h)};a.P=function(){var b=this.bB,e=b.H(),k=e=="*"?"_":"*",c=b.by;if(b.D()||ad(b.F(-1))){while(ae(b.H()))b.a5(1);if(
b.J()||ad(b.H()))return this.U(2,c,b.by-c);b.by=c}while(ae(b.F(-1)))b.a5(-1);var h=b.D()||ad(b.F(-1));b.by=c;while(b.H()
==e)b.a5(1);var j=b.by-c;while(ae(b.F(1)))b.a5(1);var g=b.J()||ad(b.H());b.by=c+j;if(h)return this.U(11,c,b.by-c);if(g)
return this.U(12,c,b.by-c);if(this.bw.ExtraMode&&e=="_")return null;return this.U(13,c,b.by-c)};a.bg=function(h,g,b,c){
var e=this.U(b.type,b.startOffset+c,b.length-c);b.length=c;g.splice(S(g,b)+1,0,e);h.splice(S(h,b)+1,0,e);return e};a.aV=
function(n,b){var m=this.bB.E,j=true;while(j){j=false;for(var h=0;h<b.length;h++){var c=b[h];if(c.type!=11&&c.type!=13)
continue;for(var k=h+1;k<b.length;k++){var g=b[k];if(g.type!=12&&g.type!=13)break;if(m.charAt(c.startOffset)!=m.charAt(g
.startOffset))continue;var e=Math.min(c.length,g.length);if(e>=3)e=e%2==1?1:2;if(c.length>e){c=this.bg(n,b,c,c.length-e)
;h--}if(g.length>e)this.bg(n,b,g,e);c.type=e==1?3:5;g.type=e==1?4:6;b.splice(S(b,c),1);b.splice(S(b,g),1);j=true;break}}
}};a.aD=function(){if(this.br)return null;var c=this.bB;c.a5(1);c.az();var j=this.bw.ExtraMode;while(!c.J()){var h=c.H()
;if(ad(h))break;if(h==">"){var b=az(c.W(),j),e=null;if(ah(b)){var g;if(b.toLowerCase().substr(0,7)=="mailto:")g=b.substr
(7);else{g=b;b="mailto:"+b}e=new am(new C("auto",b,null),g)}else if(al(b))e=new am(new C("auto",b,null),b);if(e!=null){c
.a5(1);return this.O(9,e)}return null}c.a2(j)}return null};a.aI=function(){var b=this.bB,h=b.aW("!")?10:9;if(!b.aW("["))
return null;var o=this.by;if(this.bw.ExtraMode&&h==9&&b.aW("^")){b.a8();b.az();var m=b.a4();if(m!=null&&b.aW("]")){var s
=this.bw.Q(m);if(s>=0)return this.O(14,{index:s,id:m})}this.by=o}if(this.br&&h==9)return null;var r=this.bw.ExtraMode;b.
az();var j=1;while(!b.J()){var p=b.H();if(p=="[")j++;else if(p=="]"){j--;if(j==0)break}b.a2(r)}if(b.J())return null;var 
n=az(b.W(),r);b.a5(1);o=b.by;if(b.aW("(")){var t=ar(b,null,this.bw.ExtraMode);if(t==null)return null;b.bd();if(!b.aW(")"
))return null;return this.O(h,new am(t,n))}if(!b.aW(" "))b.aW("\t");if(b.Y()){b.aZ();b.a8()}var c=null;if(b.H()=="["){b.
a5(1);b.az();if(!b.Z("]"))return null;c=b.W();b.a5(1)}else b.by=o;if(!c){c=n;while(true){var k=c.indexOf("\n");if(k<0)
break;var g=k;while(g>0&&ad(c.charAt(g-1)))g--;var e=k;while(e<c.length&&ad(c.charAt(e)))e++;c=c.substr(0,g)+" "+c.
substr(e)}}var q=this.bw.GetLinkDefinition(c);if(q==null)return null;return this.O(h,new am(q,n))};a.aF=function(){var b
=this.bB,c=b.by,e=0;while(b.aW("`"))e++;b.bd();if(b.J())return this.U(0,c,b.by-c);var g=b.by;if(!b.Z(b.E.substr(c,e)))
return this.U(0,c,b.by-c);var h=b.by+e;while(ad(b.F(-1)))b.a5(-1);var j=this.U(7,g,b.by-g);b.by=h;return j};a.U=function
(g,e,c){if(this.bG.length!=0){var b=this.bG.pop();b.type=g;b.startOffset=e;b.length=c;b.X=null;return b}else return new 
ax(g,e,c)};a.O=function(e,c){if(this.bG.length!=0){var b=this.bG.pop();b.type=e;b.X=c;return b}else{var b=new ax(e,0,0);
b.X=c;return b}};a.ak=function(b){b.X=null;this.bG.push(b)};function B(){}a=B.prototype;a.E=null;a.v=0;a.R=0;a.N=0;a.ay=
0;a.aA=0;a.C=null;a.X=null;a.an=function(){if(this.E==null)return null;if(this.R==-1)return this.E;return this.E.substr(
this.R,this.N)};a.al=function(){var c=new F();for(var b=0;b<this.C.length;b++){c.x(this.C[b].an());c.x("\n")}return c.bh
()};a.aN=function(e,c){for(var b=0;b<this.C.length;b++)this.C[b].l(e,c)};a.aX=function(e){if(this.X!=null)return this.X;
var c=at(this.E,this.R,this.ao()),b=null;if(c!=null){this.a3(c.end);b=c.id}else b=e.aC(this.E,this.R,this.N);this.X=b;
return b};a.l=function(c,b){switch(this.v){case 0:return;case 12:c.bz.ah(b,this.E,this.R,this.N);break;case 17:c.bz.ae(b
,this.E,this.R,this.N);b.x("\n");break;case 1:case 2:case 3:case 4:case 5:case 6:if(c.ExtraMode&&!c.SafeMode){b.x("<h"+(
this.v-1+1).toString());var k=this.aX(c);if(k){b.x(' id="');b.x(k);b.x('">')}else b.x(">")}else b.x("<h"+(this.v-1+1).
toString()+">");c.bz.ae(b,this.E,this.R,this.N);b.x("</h"+(this.v-1+1).toString()+">\n");break;case 14:b.x("<hr />\n");
return;case 10:case 11:b.x("<li>");c.bz.ae(b,this.E,this.R,this.N);b.x("</li>\n");break;case 15:b.x(this.E.substr(this.R
,this.N));return;case 16:b.au(this.E,this.R,this.N);return;case 18:b.x("<pre");if(c.FormatCodeBlockAttributes!=null)b.x(
c.FormatCodeBlockAttributes(this.X));b.x("><code>");var h=b;if(c.FormatCodeBlock){h=b;b=new F()}for(var e=0;e<this.C.
length;e++){var j=this.C[e];b.av(j.E,j.R,j.N);b.x("\n")}if(c.FormatCodeBlock){h.x(c.FormatCodeBlock(b.bh(),this.X));b=h}
b.x("</code></pre>\n\n");return;case 9:b.x("<blockquote>\n");this.aN(c,b);b.x("</blockquote>\n");return;case 19:b.x(
"<li>\n");this.aN(c,b);b.x("</li>\n");return;case 20:b.x("<ol>\n");this.aN(c,b);b.x("</ol>\n");return;case 21:b.x(
"<ul>\n");this.aN(c,b);b.x("</ul>\n");return;case 22:var g=this.X,n=g.name.toLowerCase();if(n=="a")c.OnPrepareLink(g);
else if(n=="img")c.OnPrepareImage(g,c.RenderingTitledImage);g.aS(b);b.x("\n");this.aN(c,b);g.aO(b);b.x("\n");return;case
 23:case 28:this.aN(c,b);return;case 24:this.X.l(c,b);return;case 25:b.x("<dd>");if(this.C!=null){b.x("\n");this.aN(c,b)
}else c.bz.ae(b,this.E,this.R,this.N);b.x("</dd>\n");break;case 26:if(this.C==null){var m=this.an().split("\n");for(var 
e=0;e<m.length;e++){var o=m[e];b.x("<dt>");c.bz.af(b,ay(o));b.x("</dt>\n")}}else{b.x("<dt>\n");this.aN(c,b);b.x(
"</dt>\n")}break;case 27:b.x("<dl>\n");this.aN(c,b);b.x("</dl>\n");return;case 29:b.x("<p>");if(this.N>0){c.bz.ae(b,this
.E,this.R,this.N);b.x("&nbsp;")}b.x(this.X);b.x("</p>\n");break}};a.aY=function(){this.v=12;this.R=this.ay;this.N=this.
aA};a.ao=function(){return this.R+this.N};a.a3=function(b){this.N=b-this.R};a.aq=function(){var c=0;for(var b=this.ay;b<
this.ay+this.aA;b++)if(this.E.charAt(b)==" ")c++;else break;return c};a.V=function(b){this.v=b.v;this.E=b.E;this.R=b.R;
this.N=b.N;this.ay=b.ay;this.aA=b.aA;return this};function D(b,c){this.bw=b;this.bx=0;this.bo=c}a=D.prototype;a.aH=
function(c){var b=new G(c);return this.a1(b)};a.aL=function(g,e,b){var c=new G(g,e,b);return this.a1(c)};a.bi=function(b
,c,e){if(e.length>1)return false;if(e.length==1){var g=b.by;b.by=e[0].ay;c.bu=c.aG(b);if(c.bu==null)return false;b.by=g;
e.length=0}while(true){var g=b.by,h=c.aG(b);if(h!=null){c.bA.push(h);continue}b.by=g;break}return true};a.a1=function(j)
{var e=[],c=[],k=-1;while(!j.J()){var m=k==0,b=this.ab(j);k=b.v;if(b.v==25)b.X=m;if(b.v==7||b.v==8){if(c.length>0){var g
=c.pop();this.S(e,c);if(g.v!=0){g.aY();g.v=b.v==7?1:2;e.push(g);continue}}if(b.v==7){b.aY();c.push(b)}else if(b.N>=3){b.
v=14;e.push(b)}else{b.aY();c.push(b)}continue}var h=c.length>0?c[0].v:0;if(b.v==24){var o=b.X,n=j.by;if(!this.bi(j,o,c))
{j.by=n;b.aY()}else{e.push(b);continue}}switch(b.v){case 0:switch(h){case 0:this.ai(b);break;case 12:this.S(e,c);this.ai
(b);break;case 9:case 10:case 11:case 25:case 28:case 13:c.push(b);break}break;case 12:switch(h){case 0:case 12:c.push(b
);break;case 9:case 10:case 11:case 25:case 28:var g=c[c.length-1];if(g.v==0){this.S(e,c);c.push(b)}else c.push(b);break
;case 13:this.S(e,c);c.push(b);break}break;case 13:switch(h){case 0:c.push(b);break;case 12:case 9:var g=c[c.length-1];
if(g.v==0){this.S(e,c);c.push(b)}else{b.aY();c.push(b)}break;case 10:case 11:case 13:case 25:case 28:c.push(b);break}
break;case 9:if(h!=9)this.S(e,c);c.push(b);break;case 10:case 11:switch(h){case 0:c.push(b);break;case 12:case 9:var g=c
[c.length-1];if(g.v==0||this.bx==10||this.bx==11||this.bx==25){this.S(e,c);c.push(b)}else{b.aY();c.push(b)}break;case 10
:case 11:if(b.v!=10&&b.v!=11)this.S(e,c);c.push(b);break;case 25:case 28:if(b.v!=h)this.S(e,c);c.push(b);break;case 13:
this.S(e,c);c.push(b);break}break;case 25:case 28:switch(h){case 0:case 12:case 25:case 28:this.S(e,c);c.push(b);break;
default:b.aY();c.push(b);break}break;default:this.S(e,c);e.push(b);break}}this.S(e,c);if(this.bw.ExtraMode)this.I(e);
return e};a.T=function(c){var b;if(this.bw.bC.length>1)b=this.bw.bC.pop();else b=new B();b.ay=c;return b};a.ai=function(
b){this.bw.bC.push(b)};a.aj=function(b){for(var c=0;c<b.length;c++)this.bw.bC.push(b[c]);b.length=0};a.aQ=function(g){
var b=this.bw.as();for(var c=0;c<g.length;c++){var e=g[c];b.x(e.E.substr(e.R,e.N));b.x("\n")}return b.bh()};a.S=function
(c,b){while(b.length>0&&b[b.length-1].v==0)this.ai(b.pop());if(b.length==0)return;switch(b[0].v){case 12:var h=this.T(b[
0].ay);h.v=12;h.E=b[0].E;h.R=b[0].R;h.a3(b[b.length-1].ao());c.push(h);this.aj(b);break;case 9:var p=this.aQ(b),o=new D(
this.bw,this.bo);o.bx=9;var n=this.T(b[0].ay);n.v=9;n.C=o.aH(p);this.aj(b);c.push(n);break;case 10:case 11:c.push(this.M
(b));break;case 25:if(c.length>0){var j=c[c.length-1];switch(j.v){case 12:j.v=26;break;case 25:break;default:var k=this.
T(j.ay);k.v=26;k.C=[];k.C.push(j);c.pop();c.push(k);break}}c.push(this.G(b));break;case 28:this.bw.z(this.L(b));break;
case 13:var e=this.T(b[0].ay);e.v=18;e.C=[];var g=b[0].an();if(g.substr(0,2)=="{{"&&g.substr(g.length-2,2)=="}}"){e.X=g.
substr(2,g.length-4);b.splice(0,1)}for(var m=0;m<b.length;m++)e.C.push(b[m]);c.push(e);b.length=0;break}};a.ab=function(
c){var b=this.T(c.by);b.E=c.E;b.R=c.by;b.N=-1;b.v=this.ac(c,b);if(b.N<0){c.bb();b.N=c.by-b.R}b.aA=c.by-b.ay;c.aZ();
return b};a.ac=function(b,c){if(b.Y())return 0;var h=b.by,e=b.H();if(e=="#"){var j=1;b.a5(1);while(b.H()=="#"){j++;b.a5(
1)}if(j>6)j=6;b.a8();c.R=b.by;b.bb();if(this.bw.ExtraMode&&!this.bw.SafeMode){var m=at(b.E,c.R,b.by);if(m!=null){c.X=m.
id;b.by=m.end}}while(b.by>c.R&&b.F(-1)=="#")b.a5(-1);while(b.by>c.R&&ad(b.F(-1)))b.a5(-1);c.N=b.by-c.R;b.bb();return 1+j
-1}if(e=="-"||e=="="){var k=e;while(b.H()==k)b.a5(1);b.a8();if(b.Y())return k=="="?7:8;b.by=h}if(this.bw.ExtraMode){var 
s=av(b);if(s!=null){c.X=s;return 24}b.by=h;if(e=="~"){if(this.aJ(b,c))return c.v;b.by=h}}var g=-1,r=0;while(!b.Y()){if(b
.H()==" "){if(g<0)r++}else if(b.H()=="\t"){if(g<0)g=b.by}else break;b.a5(1)}if(b.Y()){c.N=0;return 0}if(r>=4){c.R=h+4;
return 13}if(g>=0&&g-h<4){c.R=g+1;return 13}c.R=b.by;e=b.H();if(e=="<"){if(this.a0(b,c))return c.v;b.by=c.R}if(e==">"){
if(ab(b.F(1))){b.a5(2);c.R=b.by;return 9}b.a5(1);c.R=b.by;return 9}if(e=="-"||e=="_"||e=="*"){var o=0;while(!b.Y()){var 
k=b.H();if(b.H()==e){o++;b.a5(1);continue}if(ab(b.H())){b.a5(1);continue}break}if(b.Y()&&o>=3)return 14;b.by=c.R}if(this
.bw.ExtraMode&&e=="*"&&b.F(1)=="["){b.a5(2);b.a8();b.az();while(!b.Y()&&b.H()!="]")b.a5(1);var n=ay(b.W());if(b.H()=="]"
&&b.F(1)==":"&&n){b.a5(2);b.a8();b.az();b.bb();var v=b.W();this.bw.y(n,v);return 0}b.by=c.R}if((e=="*"||e=="+"||e=="-")
&&ab(b.F(1))){b.a5(1);b.a8();c.R=b.by;return 11}if(e==":"&&this.bw.ExtraMode&&ab(b.F(1))){b.a5(1);b.a8();c.R=b.by;
return 25}if(X(e)){b.a5(1);while(X(b.H()))b.a5(1);if(b.aW(".")&&b.a8()){c.R=b.by;return 10}b.by=c.R}if(e=="["){if(this.
bw.ExtraMode&&b.F(1)=="^"){var t=b.by;b.a5(2);var p=b.a4();if(p!=null&&b.aW("]")&&b.aW(":")){b.a8();c.R=b.by;c.X=p;
return 28}b.by=t}var q=an(b,this.bw.ExtraMode);if(q!=null){this.bw.A(q);return 0}}return 12};a.ar=function(c){var b=c.
attributes.markdown;if(b==undefined)if(this.bo)return 3;else return 0;delete c.attributes.markdown;if(b=="1")return(c.ap
()&8)!=0?2:1;if(b=="block")return 1;if(b=="deep")return 3;if(b=="span")return 2;return 4};a.aK=function(b,e,o,m){var g=b
.by,k=1,j=false;while(!b.J()){if(!b.Z("<"))break;var n=b.by,h=ag(b);if(h==null){b.a5(1);continue}if(this.bw.SafeMode&&m
==4&&!j)if(!h.at())j=true;if(h.closed)continue;if(h.name==o.name)if(h.closing){k--;if(k==0){b.a8();b.aZ();e.v=22;e.X=o;e
.a3(b.by);switch(m){case 2:var c=this.T(g);c.E=b.E;c.v=17;c.R=g;c.N=n-g;e.C=[];e.C.push(c);break;case 1:case 3:var p=new
 D(this.bw,m==3);e.C=p.aL(b.E,g,n-g);break;case 4:if(j){e.v=16;e.a3(b.by)}else{var c=this.T(g);c.E=b.E;c.v=15;c.R=g;c.N=
n-g;e.C=[];e.C.push(c)}break}return true}}else k++}return false};a.a0=function(b,c){var g=b.by,h=ag(b);if(h==null)
return false;if(h.closing)return false;var m=false;if(this.bw.SafeMode&&!h.at())m=true;var q=h.ap();if((q&1)==0)
return false;if((q&4)!=0||h.closed){b.a8();b.aZ();c.N=b.by-c.R;c.v=m?16:15;return true}if((q&2)!=0){b.a8();if(!b.Y())
return false}var o=this.bw.ExtractHeadBlocks&&h.name.toLowerCase()=="head",t=b.by;if(!o&&this.bw.ExtraMode){var n=this.
ar(h);if(n!=0)return this.aK(b,c,h,n)}var k=null,p=1;while(!b.J()){if(!b.Z("<"))break;var s=b.by,j=ag(b);if(j==null){b.
a5(1);continue}if(this.bw.SafeMode&&!j.at())m=true;if(j.closed)continue;if(!o&&!j.closing&&this.bw.ExtraMode&&!m){var n=
this.ar(j);if(n!=0){var r=this.T(g);if(this.aK(b,r,j,n)){if(k==null)k=[];if(s>g){var e=this.T(g);e.E=b.E;e.v=15;e.R=g;e.
N=s-g;k.push(e)}k.push(r);g=b.by;continue}else this.ai(r)}}if(j.name==h.name&&!j.closed)if(j.closing){p--;if(p==0){b.a8(
);b.aZ();if(m){c.v=16;c.a3(b.by);return true}if(k!=null){if(b.by>g){var e=this.T(g);e.E=b.E;e.v=15;e.R=g;e.N=b.by-g;k.
push(e)}c.v=23;c.a3(b.by);c.C=k;return true}if(o){var v=b.E.substr(t,s-t);this.bw.HeadBlockContent=this.bw.
HeadBlockContent+ay(v)+"\n";c.v=15;c.R=b.bK;c.contentEnd=b.bK;c.ay=b.bK;return true}c.v=15;c.N=b.by-c.R;return true}}
else p++}return 0};a.M=function(b){var r=b[0].v,t=b[0].aq();for(var c=1;c<b.length;c++){if(b[c].v==12&&(b[c-1].v==12||b[
c-1].v==11||b[c-1].v==10)){b[c-1].a3(b[c].ao());this.ai(b[c]);b.splice(c,1);c--;continue}if(b[c].v!=13&&b[c].v!=0){var s
=b[c].aq();if(s>t){b[c].v=13;var v=b[c].ao();b[c].R=b[c].ay+s;b[c].a3(v)}}}var h=this.T(0);h.v=r==11?21:20;h.C=[];for(
var c=0;c<b.length;c++){var k=c;while(k>0&&b[k-1].v==0)k--;var g=c;while(g<b.length-1&&b[g+1].v!=11&&b[g+1].v!=10)g++;
if(k==g)h.C.push(this.T().V(b[c]));else{var o=false,n=this.bw.as();for(var e=k;e<=g;e++){var m=b[e];n.x(m.E.substr(m.R,m
.N));n.x("\n");if(b[e].v==0)o=true}var j=this.T();j.v=19;j.ay=b[k].ay;var p=new D(this.bw);p.bx=r;j.C=p.aH(n.bh());if(!o
)for(var e=0;e<j.C.length;e++){var q=j.C[e];if(q.v==12)q.v=17}h.C.push(j)}c=g}h.ay=h.C[0].ay;this.aj(b);b.length=0;
return h};a.G=function(b){for(var c=1;c<b.length;c++)if(b[c].v==12&&(b[c-1].v==12||b[c-1].v==25)){b[c-1].a3(b[c].ao());
this.ai(b[c]);b.splice(c,1);c--;continue}var k=b[0].X;if(b.length==1&&!k){var m=b[0];b.length=0;return m}var h=this.bw.
as();for(var c=0;c<b.length;c++){var g=b[c];h.x(g.E.substr(g.R,g.N));h.x("\n")}var e=this.T(b[0].ay);e.v=25;var j=new D(
this.bw);j.bx=25;e.C=j.aH(h.bh());this.aj(b);b.length=0;return e};a.I=function(e){var c=null;for(var b=0;b<e.length;b++)
switch(e[b].v){case 26:case 25:if(c==null){c=this.T(e[b].ay);c.v=27;c.C=[];e.splice(b,0,c);b++}c.C.push(e[b]);e.splice(b
,1);b--;break;default:c=null;break}};a.L=function(c){for(var b=1;b<c.length;b++)if(c[b].v==12&&(c[b-1].v==12||c[b-1].v==
28)){c[b-1].a3(c[b].ao());this.ai(c[b]);c.splice(b,1);b--;continue}var h=this.bw.as();for(var b=0;b<c.length;b++){var g=
c[b];h.x(g.E.substr(g.R,g.N));h.x("\n")}var j=new D(this.bw);j.bx=28;var e=this.T(c[0].ay);e.v=28;e.X=c[0].X;e.C=j.aH(h.
bh());this.aj(c);c.length=0;return e};a.aJ=function(b,e){var k=b.by;b.az();while(b.H()=="~")b.a5(1);var g=b.W();if(g.
length<3)return false;b.a8();if(!b.Y())return false;b.aZ();var j=b.by;if(!b.Z(g))return false;if(!Y(b.F(-1)))
return false;var h=b.by;b.a5(g.length);b.a8();if(!b.Y())return false;e.v=18;e.C=[];h--;var c=this.T(k);c.v=13;c.E=b.E;c.
R=j;c.N=h-j;e.C.push(c);return true};function H(){this.bp=[];this.bu=null;this.bA=[]}a=H.prototype;a.ax=false;a.bk=false
;a.aG=function(b){b.a8();if(b.Y())return null;var e=this.ax;if(this.ax&&!b.aW("|")){e=true;return null}var c=[];while(!b
.Y()){b.az();while(!b.Y()&&b.H()!="|")b.a5(1);c.push(ay(b.W()));e|=b.aW("|")}if(!e)return null;while(c.length<this.bp.
length)c.push("&nbsp;");b.aZ();return c};a.aT=function(h,b,e,g){for(var c=0;c<e.length;c++){b.x("\t<");b.x(g);if(c<this.
bp.length)switch(this.bp[c]){case 1:b.x(' align="left"');break;case 2:b.x(' align="right"');break;case 3:b.x(
' align="center"');break}b.x(">");h.bz.af(b,e[c]);b.x("</");b.x(g);b.x(">\n")}};a.l=function(e,b){b.x("<table>\n");if(
this.bu!=null){b.x("<thead>\n<tr>\n");this.aT(e,b,this.bu,"th");b.x("</tr>\n</thead>\n")}b.x("<tbody>\n");for(var c=0;c<
this.bA.length;c++){var g=this.bA[c];b.x("<tr>\n");this.aT(e,b,g,"td");b.x("</tr>\n")}b.x("</tbody>\n");b.x("</table>\n"
)};function av(b){b.a8();if(b.H()!="|"&&b.H()!=":"&&b.H()!="-")return null;var c=null;if(b.aW("|")){c=new H();c.ax=true}
while(true){b.a8();if(b.H()=="|")return null;var g=b.aW(":");while(b.H()=="-")b.a5(1);var h=b.aW(":");b.a8();var e=0;if(
g&&h)e=3;else if(g)e=1;else if(h)e=2;if(b.Y()){if(c==null)return null;c.bp.push(e);return c}if(!b.aW("|"))return null;
if(c==null)c=new H();c.bp.push(e);b.a8();if(b.Y()){c.bk=true;return c}}}this.Markdown=i;this.HtmlTag=w})();
// MarkdownDeep - http://www.toptensoftware.com/markdowndeep
// Copyright (C) 2010-2011 Topten Software
var MarkdownDeepEditor=new(function(){var q=false,w={Z:"undo",Y:"redo",B:"bold",I:"italic",H:"heading",K:"code",U:
"ullist",O:"ollist",Q:"indent",E:"outdent",L:"link",G:"img",R:"hr","0":"h0","1":"h1","2":"h2","3":"h3","4":"h4","5":"h5"
,"6":"h6"};function A(d,b){return d.substr(0,b.length)==b}function t(d,b){return d.substr(-b.length)==b}function v(b){
return b==" "||b=="\t"||b=="\r"||b=="\n"}function x(b){return b=="\r"||b=="\n"}function y(e){var b=0,d=e.length;while(b<
d&&v(e.charAt(b)))b++;while(d-1>b&&v(e.charAt(d-1)))d--;return e.substr(b,d-b)}function s(b,d,e){if(b.addEventListener)b
.addEventListener(d,e,false);else if(b.attachEvent)b.attachEvent("on"+d,e)}function B(b,d,e){if(b.removeEventListener)b.
removeEventListener(d,e,false);else if(b.detachEvent)b.detachEvent("on"+d,e)}function u(b){if(b.preventDefault)b.
preventDefault();if(b.cancelBubble!==undefined){b.cancelBubble=true;b.keyCode=0;b.returnValue=false}return false}
function z(d,b){return b-d.value.slice(0,b).split("\r\n").length+1}function p(){}a=p.prototype;a.D=function(b){this.aa=b
;if(q){var d=document.selection.createRange(),f=d.duplicate();f.moveToElementText(b);var e=-f.moveStart("character",-
10000000);this.Z=-d.moveStart("character",-10000000)-e;this.Y=-d.moveEnd("character",-10000000)-e;this.ad=b.value.
replace(/\r\n/gm,"\n")}else{this.Z=b.selectionStart;this.Y=b.selectionEnd;this.ad=b.value}};a.u=function(){var b=new p()
;b.aa=this.aa;b.Y=this.Y;b.Z=this.Z;b.ad=this.ad;return b};a.m=function(){if(q){this.aa.value=this.ad;this.aa.focus();
var b=this.aa.createTextRange();b.collapse(true);b.moveEnd("character",this.Y);b.moveStart("character",this.Z);b.select(
)}else{var d=this.aa.scrollTop;this.aa.value=this.ad;this.aa.focus();this.aa.setSelectionRange(this.Z,this.Y);this.aa.
scrollTop=d}};a.J=function(b){this.ad=this.ad.substr(0,this.Z)+b+this.ad.substr(this.Y);this.Y=this.Z+b.length};function
 r(b,d,e,f){if(b<d)return b;return b<d+e?d:b+f-e}a.I=function(b,d,e){this.ad=this.ad.substr(0,b)+e+this.ad.substr(b+d);
this.Z=r(this.Z,b,d,e.length);this.Y=r(this.Y,b,d,e.length)};a.t=function(){return this.ad.substr(this.Z,this.Y-this.Z)}
;a.C=function(d,b){this.Y+=b;this.Z-=d};a.G=function(b){return this.Z>=b.length&&this.ad.substr(this.Z-b.length,b.length
)==b};a.s=function(b){return this.ad.substr(this.Y,b.length)==b};a.U=function(){while(v(this.ad.charAt(this.Z)))this.Z++
;while(this.Y>this.Z&&v(this.ad.charAt(this.Y-1)))this.Y--};a.E=function(b){return b==0||x(this.ad.charAt(b-1))};a.p=
function(b){while(b>0&&!x(this.ad.charAt(b-1)))b--;return b};a.r=function(b){while(b<this.ad.length&&!x(this.ad.charAt(b
)))b++;return b};a.w=function(b){return this.P(this.r(b))};a.T=function(b){while(b<this.ad.length&&v(this.ad.charAt(b)))
b++;return b};a.P=function(b){if(this.ad.substr(b,2)=="\r\n")return b+2;if(x(this.ad.charAt(b)))return b+1;return b};a.R
=function(b){if(b>2&&this.ad.substr(b-2,2)=="\r\n")return b-2;if(b>1&&x(this.ad.charAt(b-1)))return b-1;return b};a.M=
function(){this.Z=this.p(this.Z);if(!this.E(this.Y))this.Y=this.P(this.r(this.Y))};a.S=function(b){while(b>0&&v(this.ad.
charAt(b-1)))b--;return b};a.Q=function(b){while(v(this.ad.charAt(b)))b++;return b};a.L=function(){this.Z=this.S(this.Z)
;this.Y=this.Q(this.Y)};a.o=function(){var d=this.t(),b=d.match(/\n[ \t\r]*\n/);if(b){alert(
"Please make a selection that doesn't include a paragraph break");return false}return true};a.B=function(f){var e=this.
ad.length;for(var b=f;b<e;b++){var d=this.ad[b];if(x(d))return true;if(!v(this.ad.charAt(b)))return false}return true};a
.y=function(b){var e=b;b=this.p(b);if(this.B(b))return b;while(b>0){var d=this.p(this.R(b));if(d==0)break;if(this.B(d))
break;b=d}if(this.q(b).af!=0){b=this.p(e);while(b>0){if(this.q(b).af!=0)return b;b=this.p(this.R(b))}}return b};a.v=
function(b){while(b<this.ad.length){if(this.B(b))break;b=this.w(b)}return b};a.K=function(){this.Z=this.y(this.Z);this.Y
=this.v(this.Z)};a.q=function(d){var e=this.ad.substr(d,10),b=e.match(/^\s{0,3}(\*|\d+\.)(?:\ |\t)*/);if(!b)return{ab:""
,af:0};if(b[1]=="*")return{ab:"*",af:b[0].length};else return{ab:"1",af:b[0].length}};function l(b,e){if(!b.
setSelectionRange)q=true;this.X=null;this.ag=[];this.ae=0;this.ac=3;this.Markdown=new MarkdownDeep.Markdown();this.
Markdown.SafeMode=false;this.Markdown.ExtraMode=true;this.Markdown.NewWindowForLocalLinks=true;this.Markdown.
NewWindowForExternalLinks=true;this.aa=b;this.W=e;var f=this;s(b,"keyup",function(){f.H()});s(b,"keydown",function(d){
return f.F(d)});s(b,"paste",function(){f.H()});s(b,"input",function(){f.H()});s(b,"mousedown",function(){f.O(3)});this.H
()}var a=l.prototype,c=l.prototype;a.F=function(b){var d=null,f=true;if(b.ctrlKey||b.metaKey){var e=String.fromCharCode(
b.charCode||b.keyCode);if(!this.disableShortCutKeys&&w[e]!=undefined){this.InvokeCommand(w[e]);return u(b)}switch(e){
case"V":d=1;break;case"X":d=2;break}}else switch(b.keyCode){case 9:if(!this.disableTabHandling){this.InvokeCommand(b.
shiftKey?"untab":"tab");return u(b)}else d=1;break;case 37:case 39:case 38:case 40:case 36:case 35:case 33:case 34:d=3;
break;case 8:case 46:d=2;break;case 13:d=4;break;default:d=1}if(d!=null)this.O(d);if(!this.disableAutoIndent)if(b.
keyCode==13&&(!q||b.ctrlKey))this.IndentNewLine()};a.O=function(b){if(this.ac==b)return;this.ac=b;this.n()};a.n=function
(){var b=new p();b.D(this.aa);this.ag.splice(this.ae,this.ag.length-this.ae,b);this.ae=this.ag.length};a.H=function(e){
var b=this.aa.value;if(b===this.X&&this.X!==null)return;if(this.onPreTransform)this.onPreTransform(this,b);var d=this.
Markdown.Transform(b);if(this.onPostTransform)this.onPostTransform(this,d);if(this.W)this.W.innerHTML=d;if(this.
onPostUpdateDom)this.onPostUpdateDom(this);this.X=b};c.onOptionsChanged=function(){this.X=null;this.H()};c.cmd_undo=
function(){if(this.ae>0){if(this.ae==this.ag.length){this.n();this.ae--}this.ae--;this.ag[this.ae].m();this.ac=0;this.H(
)}};c.cmd_redo=function(){if(this.ae+1<this.ag.length){this.ae++;this.ag[this.ae].m();this.ac=0;this.H();if(this.ae==
this.ag.length-1)this.ag.pop()}};a.N=function(d,f){d.K();d.L();var b=d.t();b=y(b);var g=0,e=b.match(/^(\#+)(.*?)(\#+)?$/
);if(e){b=y(e[2]);g=e[1].length}else{e=b.match(/^(.*?)(?:\r\n|\n|\r)\s*(\-*|\=*)$/);if(e){b=y(e[1]);g=e[2].charAt(0)==
"="?1:0}else{b=b.replace(/(\r\n|\n|\r)/gm,"");g=0}}if(f==-1)f=(g+1)%4;var h=0,j=0;if(f==0){if(b=="Heading"){d.J("");
return true}j=b.length;h=0}else{if(b=="")b="Heading";h=f+1;j=b.length;var i="";for(var k=0;k<f;k++)i+="#";b=i+" "+b+" "+
i}b+="\n\n";if(d.Z!=0){b="\n\n"+b;h+=2}d.J(b);d.Z+=h;d.Y=d.Z+j;return true};c.cmd_heading=function(b){return this.N(b,-1
)};c.cmd_h0=function(b){return this.N(b,0)};c.cmd_h1=function(b){return this.N(b,1)};c.cmd_h2=function(b){return this.N(
b,2)};c.cmd_h3=function(b){return this.N(b,3)};c.cmd_h4=function(b){return this.N(b,4)};c.cmd_h5=function(b){return this
.N(b,5)};c.cmd_h6=function(b){return this.N(b,6)};a.x=function(j,h){j.M();var d=j.t().split("\n");for(var b=0;b<d.length
;b++)if(d[b].charAt(0)=="\t"){var f="",e=0;while(d[b].charAt(e)=="\t"){f+="    ";e++}var i=f+d[b].substr(e);d.splice(b,1
,i)}if(h===null){var b;for(b=0;b<d.length;b++){if(y(d[b])=="")continue;if(d[b].charAt(0)=="\t"){var f="",e=0;while(d[b].
charAt(e)=="\t"){f+="    ";e++}var i=f+d[b].substr(b);d.splice(b,1,i)}if(!A(d[b],"    "))break}h=b!=d.length}for(var b=0
;b<d.length;b++){if(y(d[b])=="")continue;var g=d[b];if(h)g="    "+d[b];else if(A(d[b],"\t"))g=d[b].substr(1);else if(A(d
[b],"    "))g=d[b].substr(4);d.splice(b,1,g)}j.J(d.join("\n"))};c.cmd_code=function(b){if(b.Z==b.Y){var d=b.p(b.Z);if(b.
B(d)){b.L();b.J("\n\n    Code\n\n");b.Z+=6;b.Y=b.Z+4;return true}}if(b.t().indexOf("\n")<0){b.U();if(b.G("`"))b.Z--;if(b
.s("`"))b.Y++;return this.k(b,"`")}this.x(b,null);return true};c.cmd_tab=function(b){if(b.t().indexOf("\n")>0)this.x(b,
true);else{var e=b.p(b.Z),d;for(d=e;d<b.Z;d++)if(b.ad.charAt(d)!=" ")break;if(d==b.Z){var f=4-(d-e)%4;b.J("    ".substr(
0,f))}else b.J("\t");b.Z=b.Y}return true};c.cmd_untab=function(b){if(b.t().indexOf("\n")>0){this.x(b,false);return true}
return false};a.k=function(d,e){var g=d.ad,f=e.length,b=d.t();if(A(b,e)&&t(b,e))d.J(b.substr(f,b.length-f*2));else{d.U()
;b=d.t();if(!b)b="text";else b=b.replace(/(\r\n|\n|\r)/gm,"");d.J(e+b+e);d.C(-f,-f)}return true};c.cmd_bold=function(b){
if(!b.o())return false;b.U();if(b.G("**"))b.Z-=2;if(b.s("**"))b.Y+=2;return this.k(b,"**")};c.cmd_italic=function(b){if(
!b.o())return false;b.U();if(b.G("*")&&!b.G("**")||b.G("***"))b.Z-=1;if(b.s("*")&&!b.G("**")||b.s("***"))b.Y+=1;
return this.k(b,"*")};a.A=function(b,g){if(false&&b.Z==b.Y){b.L();b.J("\n\n> Quote\n\n");b.Z+=4;b.Y=b.Z+5;return true}b.
M();var e=b.t().split("\n");for(var d=0;d<e.length-1;d++){var f=e[d];if(g){if(A(e[d],"> "))f=e[d].substr(2)}else f="> "+
e[d];e.splice(d,1,f)}b.J(e.join("\n"));return true};c.cmd_indent=function(b){return this.A(b,false)};c.cmd_outdent=
function(b){return this.A(b,true)};a.z=function(b,o){var g=[];if(b.t().indexOf("\n")>0){b.M();var f=b.Z;g.push(f);while(
true){f=b.w(f);if(f>=b.Y)break;g.push(f)}}else g.push(b.p(b.Z));var n=o=="*"?"* ":"1. ";for(var d=0;d<g.length;d++){var 
h=b.q(g[d]);if(h.ab==o){n="";break}}for(var d=g.length-1;d>=0;d--){var f=g[d],h=b.q(f);b.I(f,h.af,n)}var j=new 
MarkdownDeep.Markdown();j.ExtraMode=true;var e=j.GetListItems(b.ad,b.Z);while(e!=null){var i=0;for(var d=0;d<e.length-1;
d++){var h=b.q(e[d]+i);if(h.ab!="1")break;var m=(d+1).toString()+". ";b.I(e[d]+i,h.af,m);i+=m.length-h.af}var k=j.
GetListItems(b.ad,e[e.length-1]+i);if(k!=null&&k[0]!=e[0])e=k;else e=null}if(g.length>1)b.M();return true};c.cmd_ullist=
function(b){return this.z(b,"*")};c.cmd_ollist=function(b){return this.z(b,"1")};c.cmd_link=function(b){b.U();if(!b.o())
return false;var e=prompt("Enter the target URL:");if(e===null)return false;var d=b.t();if(d.length==0)d="link text";var
 f="["+d+"]("+e+")";b.J(f);b.Z++;b.Y=b.Z+d.length;return true};c.cmd_img=function(b){b.U();if(!b.o())return false;var e=
prompt("Enter the image URL");if(e===null)return false;var d=b.t();if(d.length==0)d="Image Text";var f="!["+d+"]("+e+")"
;b.J(f);b.Z+=2;b.Y=b.Z+d.length;return true};c.cmd_hr=function(b){b.L();if(b.Z==0)b.J("----------\n\n");else b.J(
"\n\n----------\n\n");b.Z=b.Y;return true};c.IndentNewLine=function(){var i=this,g,h=function(){window.clearInterval(g);
var b=new p();b.D(i.aa);var e=b.p(b.R(b.Z)),d=e;while(true){var f=b.ad.charAt(d);if(f!=" "&&f!="\t")break;d++}if(d>e){b.
J(b.ad.substr(e,d-e));b.Z=b.Y}b.m()};g=window.setInterval(h,1);return false};c.cmd_indented_newline=function(b){b.J("\n"
);b.Z=b.Y;var e=b.p(b.R(b.Z)),d=e;while(true){var f=b.ad.charAt(d);if(f!=" "&&f!="\t")break;d++}if(d>e){b.J(b.ad.substr(
e,d-e));b.Z=b.Y}return true};c.InvokeCommand=function(b){if(b=="undo"||b=="redo"){this["cmd_"+b]();this.aa.focus();
return}var d=new p();d.D(this.aa);var e=d.u();if(this["cmd_"+b](d)){this.ac=0;this.ag.splice(this.ae,this.ag.length-this
.ae,e);this.ae++;d.m();this.H();return true}else{this.aa.focus();return false}};delete a;delete c;this.Editor=l})();
// MarkdownDeep - http://www.toptensoftware.com/markdowndeep
// Copyright (C) 2010-2011 Topten Software
var MarkdownDeepEditorUI=new(function(){this.HelpHtmlWritten=false;this.HelpHtml=function(b){var a="";a+=
'<div class="mdd_modal" id="mdd_syntax_container" style="display:none">\n';a+='<div class="mdd_modal_frame">\n';a+=
'<div class="mdd_modal_button">\n';a+='<a href="'+b+'" id="mdd_help_location" style="display:none"></a>\n';a+=
'<a href="#" id="mdd_close_help">Close</a>\n';a+="</div>\n";a+='<div class="mdd_modal_content">\n';a+=
'<div class="mdd_syntax" id="mdd_syntax">\n';a+='<div class="mdd_ajax_loader"></div>\n';a+="</div>\n";a+="</div>\n";a+=
"</div>\n";a+="</div>\n";return a};this.ToolbarHtml=function(){var a="";a+='<div class="mdd_links">\n';a+=
'<a href="#" class="mdd_help" tabindex=-1>How to Format</a>\n';a+="</div>\n";a+="<ul>\n";a+=
'<li><a href="#" class="mdd_button" id="mdd_undo" title="Undo (Ctrl+Z)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_redo" title="Redo (Ctrl+Y)" tabindex=-1></a></li>\n';a+=
'<li><span class="mdd_sep"></span></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_heading" title="Change Heading Style (Ctrl+H, or Ctrl+0 to Ctrl+6)" tabindex=-1></a></li>\n'
;a+=
'<li><a href="#" class="mdd_button" id="mdd_code" title="Preformatted Code (Ctrl+K or Tab/Shift+Tab on multiline selection)" tabindex=-1></a></li>\n'
;a+='<li><span class="mdd_sep"></span></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_bold" title="Bold (Ctrl+B)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_italic" title="Italic (Ctrl+I)" tabindex=-1></a></li>\n';a+=
'<li><span class="mdd_sep"></span></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_ullist" title="Bullets (Ctrl+U)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_ollist" title="Numbering (Ctrl+O)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_outdent" title="Unquote (Ctrl+W)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_indent" title="Quote (Ctrl+Q)" tabindex=-1></a></li>\n';a+=
'<li><span class="mdd_sep"></span></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_link" title="Insert Hyperlink (Ctrl+L)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_img" title="Insert Image (Ctrl+G)" tabindex=-1></a></li>\n';a+=
'<li><a href="#" class="mdd_button" id="mdd_hr" title="Insert Horizontal Rule (Ctrl+R)" tabindex=-1></a></li>\n';a+=
"</ul>\n";a+='<div style="clear:both"></div>\n';return a};this.onResizerMouseDown=function(a){var h=window.event?a.
srcElement:a.target,f=$(h).closest(".mdd_resizer_wrap").prev(".mdd_editor_wrap").children("textarea")[0],l=a.clientY,k=$
(f).height();$(document).bind("mousemove.mdd",e);$(document).bind("mouseup.mdd",g);return false;function g(b){$(document
).unbind("mousemove.mdd");$(document).unbind("mouseup.mdd");return false}function e(c){var b=k+c.clientY-l;if(b<50)b=50;
$(f).height(b);return false}};var j=0,i=false;this.onShowHelpPopup=function(){$("#mdd_syntax_container").fadeIn("fast");
$(".modal_content").scrollTop(j);$(document).bind("keydown.mdd",function(k){if(k.keyCode==27){MarkdownDeepEditorUI.
onCloseHelpPopup();return false}});if(!i){i=true;var a=$("#mdd_help_location").attr("href");if(!a)a="mdd_help.htm";$(
"#mdd_syntax").load(a)}return false};this.onCloseHelpPopup=function(){j=$(".modal_content").scrollTop();$(
"#mdd_syntax_container").fadeOut("fast");$(document).unbind("keydown.mdd");$(document).unbind("scroll.mdd");return false
};this.onToolbarButton=function(a){var b=$(a.target).closest("div.mdd_toolbar_wrap").next(".mdd_editor_wrap").children(
"textarea").data("mdd");b.InvokeCommand($(a.target).attr("id").substr(4));return false}})();(function(a){a.fn.
MarkdownDeep=function(f){var h={resizebar:true,toolbar:true,help_location:"mdd_help.html"};if(f)a.extend(h,f);
return this.each(function(){var d=a(this).parent(".mdd_editor_wrap");if(d.length==0)d=a(this).wrap(
'<div class="mdd_editor_wrap" />').parent();if(h.toolbar){var k=d.prev(".mdd_toolbar_wrap"),c=d.prev(".mdd_toolbar");if(
k.length==0){if(c.length==0){c=a('<div class="mdd_toolbar" />');c.insertBefore(d)}k=c.wrap(
'<div class="mdd_toolbar_wrap" />').parent()}else if(c.length==0){c=a('<div class="mdd_toolbar" />');k.html(c)}c.append(
a(MarkdownDeepEditorUI.ToolbarHtml()));a("a.mdd_button",c).click(MarkdownDeepEditorUI.onToolbarButton);a("a.mdd_help",c)
.click(MarkdownDeepEditorUI.onShowHelpPopup);if(!MarkdownDeepEditorUI.HelpHtmlWritten){var l=a(MarkdownDeepEditorUI.
HelpHtml(h.help_location));l.appendTo(a("body"));a("#mdd_close_help").click(MarkdownDeepEditorUI.onCloseHelpPopup);
MarkdownDeepEditorUI.HelpHtmlWritten=true}}var b,e;if(h.resizebar){e=d.next(".mdd_resizer_wrap"),b=e.length==0?d.next(
".mdd_resizer"):e.children(".mdd_resizer");if(e.length==0){if(b.length==0){b=a('<div class="mdd_resizer" />');b.
insertAfter(d)}e=b.wrap('<div class="mdd_resizer_wrap" />').parent()}else if(b.length==0){b=a(
'<div class="mdd_resizer" />');e.html(b)}e.bind("mousedown",MarkdownDeepEditorUI.onResizerMouseDown)}var j=a(this).attr(
"data-mdd-preview");if(!j)j=".mdd_preview";var i=a(j)[0];if(!i){a('<div class="mdd_preview"></div>').insertAfter(b?b:
this);i=a(".mdd_preview")[0]}var g=new MarkdownDeepEditor.Editor(this,i);if(f){jQuery.extend(g.Markdown,f);jQuery.extend
(g,f)}g.onOptionsChanged();a(this).data("mdd",g)})}})(jQuery)