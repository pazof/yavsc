/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    //  concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    shell = require("gulp-shell"),
    rename = require('gulp-rename');

var webroot = "./wwwroot/";

var paths = {
    bower: "./bower_components/",
    js: webroot + "js/**/*.js",
    minJs: webroot + "js/**/*.min.js",
    css: webroot + "css/**/*.css",
    minCss: webroot + "css/**/*.min.css",
    concatJsDest: webroot + "js/site.min.js",
    concatCssDest: webroot + "css/site.min.css"
};

gulp.task("clean:js", function(cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function(cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);



gulp.task('watch', shell.task(['ASPNET_ENV=Development dnx-watch web --configuration=Debug']));
gulp.task('watchlua', shell.task(['ASPNET_ENV=lua dnx-watch luatest --configuration=Debug']));
gulp.task('watchpre', shell.task(['ASPNET_ENV=yavscpre dnx-watch web --configuration=Debug']));

gulp.task("min:css", function() {
    gulp.src([paths.css, "!" + paths.minCss, '!site.css'])
        .pipe(cssmin())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('wwwroot/css'));
});

gulp.task("min:js", function() {
    return gulp.src([paths.js, "!" + paths.minJs, '!site.js'])
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('wwwroot/js'));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task('build', shell.task(['dnu build --configuration=Debug']));
gulp.task('run', shell.task(['ASPNET_ENV=Development dnx web --configuration=Debug']));
gulp.task('buildrelease', shell.task(['dnu build --configuration=Release']));
gulp.task('publish', shell.task(['dnu publish --configuration=Release']));
gulp.task('postpublish', shell.task(['contrib/rsync-to-pre.sh']));

gulp.task("default", ["watch"]);