

all: css js
	dotnet build

%.css: %.scss
	scss $^ > $@

%.min.js: %.js
	jsmin < $^ > $@

%.min.css: %.css
	jsmin < $^ > $@

css: wwwroot/css/site.min.css

js: wwwroot/js/site.min.js
