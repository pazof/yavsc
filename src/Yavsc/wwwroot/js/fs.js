// requires DropZone ª toMarkdown

window.RemoteFS = (function ($) {
  var Combine = function (patha, pathb) {
    if (!patha) return pathb;
    if (!pathb) return patha;
    return patha + '/' + pathb;
  };

  var OpenDir = function ($view, sub) {
    $view.data('path', sub);
    InitDir($view);
  };

    var InitDir = function ($view) {

      var path = $view.data('path');
      var owner = $view.data('owner');
      var url = path ? '/api/fs/' + path : '/api/fs';

      $view.empty();
        $.get(url, function(data) {
          
            $('<button>' + owner + '</button>').click(function() {
              OpenDir($view, null);
            }).appendTo($view);

            var npath = null;

            if (path) $.each(path.split('/'), function () {
              var part = this;
              if (npath) npath = npath + '/' + part;
              else npath = part;
              $('<button>').append(part).click(function() {
                OpenDir($view, npath)
              }).appendTo($view)
            });

            $.each(data.SubDirectories, function () {
              var item = this;
              var spath = (path) ? path + '/' + item.Name : item.Name;
              $('<button>').append(item.Name).click(function() {
                OpenDir($view, spath);
              }).appendTo($view);
            });
            var $ftable = $('<table>').append('<tr class="fileheaders"><th>Nom</th><th>Taille</th><th>Modification</th></tr>');
            $.each(data.Files, function () {
              var item = this;
              var $tr = $('<tr></tr>');
              var $td = $('<td></td>')
              $('<a></a>').append(item.Name).click(function() {
                document.location = '/' + owner + '/' + npath + '/' + item.Name
              }).appendTo($td);
              $td.appendTo($tr);
              $('<td>' + item.Size + '</td>').appendTo($tr);
              $('<td>' + item.LastModified + '</td>').appendTo($tr);
              $tr.appendTo($ftable)
            });
            $ftable.appendTo($view);
        })
    };

  $(document).ready(function ($) {
    OpenDir($('.dirinfo'));
  });

})(window.jQuery);

Dropzone.options.postfiles= {
    maxFilesize: 20, // MB   TODO: let sell it.
    autoProcessQueue: true,
    accept: function(file, done) {
      if (file.name == "justinbieber.jpg") {
        done("Naha, you don't.")
      }
      else { done() }
    },
    success: function (file, response) {
        console.log('response:');
        console.log(response);
        for (var i = 0; i < response.length; i++) {
            var filer = response[i];
            console.log('response item:');
            console.log(filer);
  
            $('<p><a href="/files/@User.GetUserName()/' + filer.FileName + '">' + filer.FileName + '</a></p>').appendTo('#ql-editor-2');
            updateMD ('Content', $('#contentview').html())
        }
    },
    url: '/api/fs'
  };


