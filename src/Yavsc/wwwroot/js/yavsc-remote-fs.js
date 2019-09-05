// requires DropZone ª toMarkdown
if (typeof jQuery === 'undefined') {
  throw new Error('yavsc-remote-fs script requires jQuery');
}
if (typeof Dropzone === 'undefined') {
  throw new Error('yavsc-remote-fs requires Dropzone');
}
if (typeof updateMD === 'undefined') {
  throw new Error('yavsc-remote-fs requires md-helpers.js');
}

(function($, Dropzone, updateMD) {

  window.RemoteFS = (function ($) {

    /*
    var Combine = function (patha, pathb) {
      if (!patha) return pathb;
      if (!pathb) return patha;
      return patha + '/' + pathb;
    }; */

    var OpenDir = function ($view, sub) {
      $view.data('path', sub);
      InitDir($view);
    };
    var root;
    var selection = [];

    var SetItemSelected = function (name, selected) {
      if (selected) selection.push(name);
      else selection = selection.filter(function(ele) {
        return ele != name;
      });
    };

    var RemoveSelectedFiles = function () {
      var xmlhttp = new XMLHttpRequest();
      $.each(selection, function() {
        xmlhttp.open('DELETE', '/api/fs/' + this, true);
        xmlhttp.send();
      });
    };

    var InitDir = function ($view) {

      root = $view.data('path');
      var owner = $view.data('owner');
      var fsiourl = root ? '/api/fs/' + root : '/api/fs';

      $view.empty();

      $.get(fsiourl, function(data) {
        $('<button>' + owner + '</button>').click(function() {
          OpenDir($view, null);
        }).appendTo($view);

        var npath = null;

        if (root) $.each(root.split('/'), function () {
          var part = this;
          if (npath == null) npath = part;
          else npath = npath + '/' + part;
          $('<button/>').append(part).click(function() {
            OpenDir($view, npath);
          }).appendTo($view);
        });

        $.each(data.SubDirectories, function () {
          var item = this;
          var spath = (root) ? root + '/' + item.Name : item.Name;
          $('<button/>').append(item.Name).click(function() {
            OpenDir($view, spath);
          }).appendTo($view);
        });
        var $divedit = $('<div></div>');
        $('<button>&#xe083;</button>').addClass('glyphicon').append().click(function() {
          RemoveSelectedFiles();
        }).appendTo($divedit);
        $divedit.appendTo($view);
        var $ftable = $('<table border="1">').css('border-spacing', '6px')
        .css('border-collapse', 'separate')
        .append('<tr class="fileheaders"><th>Nom</th><th>Taille</th><th>Modification</th></tr>');
        $.each(data.Files, function () {
          var item = this;
          var $tr = $('<tr class="fileentry"></tr>');
          var $td = $('<td></td>');
          $('<input type="checkbox" />').addClass('check-box').click(function() {
            SetItemSelected(item.Name, this.checked)
          }).appendTo($td);
          $td.append('&nbsp;');
          $('<a></a>').append(item.Name).click(function() {
            if (root) document.location = '/' + owner + '/' + root + '/' + item.Name;
            else document.location = '/files/' + owner + '/' + item.Name;
          }).appendTo($td);
          $td.appendTo($tr);
          $('<td>' + item.Size + '</td>').appendTo($tr);
          $('<td>' + item.LastModified + '</td>').appendTo($tr);
          $tr.appendTo($ftable);
        });
        $ftable.appendTo($view);
      });
    };
    $(document).ready(function () {
      OpenDir($('.dirinfo'));
    });
  })($);


  Dropzone.options.postfiles = {
    maxFilesize: 20, // MB   TODO: let sell it.
    autoProcessQueue: true,
    accept: function(file, done) {
      if (file.name == 'justinbieber.jpg') {
        done('Naha, you don\'t.');
      } else { done(); }
    },
    success: function (file, response) {
        for (var i = 0; i < response.length; i++) {
            var filer = response[i];
            $('<p><a href="/files/@User.GetUserName()/' + filer.FileName + '">' + filer.FileName + '</a></p>').appendTo('#ql-editor-2');
            updateMD('Content', $('#contentview').html());
        }
    },
    url: '/api/fs'
  };
})($, Dropzone, updateMD);

