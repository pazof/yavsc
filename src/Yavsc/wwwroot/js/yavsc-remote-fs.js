// requires DropZone ª toMarkdown

if (typeof window.jQuery === 'undefined') {
  throw new Error('yavsc-remote-fs script requires jQuery');
}
if (typeof XMLHttpRequest === 'undefined') {
  throw new Error('yavsc-remote-fs script requires XMLHttpRequest');
}

(function ($) {
  'use strict';
  $.widget('psc.yarfs', {
    options: {
      fsnurl: '/api/fs'
    },
    root: null,
    rmDialog: null,
    mvDialog: null,
    flist: null,
    selection: [],
    dirBar: null,
    destination: null,
    rootDisplay: null,
    setRoot: function(sub) {
      this.root = sub;
      if (!this.root) this.rootDisplay.addClass('hidden');
      else 
      {
        this.rootDisplay.removeClass('hidden');
        this.rootDisplay.html('from <code>' + this.root + '</code>');
      }
    },
    openDir: function (sub) {
      var _this = this;
      this.setRoot(sub);
      var owner = this.element.data('owner');
      this.selection = [];
      this.dirBar.empty();
      $('<button>' + owner + '</button>')
        .click(function () {
          _this.openDir(null);
        })
        .appendTo(this.dirBar);
      var npath = null

      if (_this.root) {
        var dnames = _this.root.split('/');
        $.each(dnames, function () {
          var part = this;
          if (npath == null) npath = part;
          else npath = npath + '/' + part;
          $('<button/>')
            .append(part)
            .click(function () {
              _this.OpenDir(npath);
            })
            .appendTo(this.dirBar);
        });
      }

      this.ftable.find('tr.fileinfo').remove();
      var fsiourl = this.root ? '/api/fs/' + this.root : '/api/fs';
      $.get(fsiourl, function (data) {
        $.each(data.SubDirectories, function () {
          var item = this;
          var spath = _this.root ? _this.root + '/' + item.Name : item.Name;
          $('<button/>')
            .append(item.Name)
            .click(function () {
              _this.openDir(spath);
            })
            .appendTo(_this.dirBar);
        });

        $.each(data.Files, function () {
          var item = this;
          var $tr = $('<tr class="fileinfo"></tr>');
          var $td = $('<td></td>');
          $td.appendTo($tr);
          $('<input type="checkbox" />')
            .addClass('check-box')
            .click(function () {
              _this.SetItemSelected(item.Name, this.checked);
            })
            .appendTo($td);

          $('<td></td>')
            .append($('<a></a>')
              .append(item.Name)
              .click(function () {
                if (_this.root) document.location = '/' + owner + '/' + _this.root + '/' + item.Name;
                else document.location = '/files/' + owner + '/' + item.Name;
              })).appendTo($tr);
          $('<td>' + item.Size + '</td>').appendTo($tr);
          $('<td>' + item.LastModified + '</td>').appendTo($tr);
          $tr.appendTo(_this.ftable);
        });
      });
    },
    SetItemSelected: function (name, selected) {
      if (selected) {
        this.selection.push(name);
      } else {
        this.selection = this.selection.filter(function (ele) {
          return ele !== name;
        });
      }
    },
    RemoveSelectedFiles: function () {
      $.each(this.selection, function () {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.open('DELETE', '/api/fs/' + this, true);
        xmlhttp.send();
      });
      this.selection = [];
      // FIXME this could fail for a very long list of big files
      setTimeout(500, function () {
        this.openDir(this.root);
      });
    },
    moveSelectedFiles: function () {
      var _this = this;
      var dest = this.destination;
      $.each(this.selection, function () {
        var data = {};
        data['id'] = _this.root ? _this.root + '/' + this : this;
        data['to'] = dest;
        console.log(data);
        var request = $.ajax({
          url: '/api/fsc/mvftd',
          type: 'POST',
          data: JSON.stringify(data),
          contentType: 'application/json;charset=utf-8'
        });

        request.done(function( msg ) {
          $( "#log" ).html( msg );
        });
         
        request.fail(function( jqXHR, textStatus, msg ) {
          alert( 'Request failed: ' + textStatus );
          $( '#log' ).html( msg );
        });
      });
      this.selection = [];
      // FIXME this could fail for a very long list of big files
      setTimeout(500, function () {
        this.openDir(this.root);
      });
    },
    askForRemoval: function () {
      this.flist.empty();
      var _this = this;
      $.each(this.selection, function () {
        _this.flist.append('<li>' + this + '</li>');
      });
      this.rmDialog.modal({ show: true });
    },
    askForMoving: function () {
      this.flist.empty();
      var _this = this;
      $.each(this.selection, function () {
        _this.flist.append('<li>' + this + '</li>');
      });
      this.mvDialog.modal({ show: true });
    },
    createRmDialog: function () {
      var _this = this;
      this.rmDialog = $('<div id="rmDialog" tabindex="-1" role="dialog"></div>');
      this.rmDialog.addClass('modal');
      this.rmDialog.addClass('fade');
      var md = $('<div role="document"></div>');
      md.addClass('modal-dialog');
      this.rmDialog.append(md);
      var mdCnt = $('<div class="modal-content"></div>');
      mdCnt.addClass('modal-content');
      var mdHeader = $('<div class="modal-header"></div>');
      mdHeader.append('<h5 class="modal-title">File removal</h5>');
      mdHeader.append('<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>');
      mdCnt.append(mdHeader);
      var mdBody = $('<div class="modal-body"></div>');
      mdBody.append('<p>You´re about to remove these files :</p>');
      this.flist = $('<ul></ul>');
      mdBody.append(this.flist);
      mdCnt.append(mdBody);
      var rmcBtn = $('<button type="button" data-dismiss="modal" class="btn btn-primary">Do deletion</button>').click(function () {
        _this.RemoveSelectedFiles();
      });
      var mdFooter = $('<div class="modal-footer"></div>');
      mdFooter.append(rmcBtn);
      mdFooter.append('<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>');
      mdCnt.append(mdFooter);
      md.append(mdCnt);
      this.rmDialog.appendTo(this.element);
    },
    onDestinationChanged: function (newDest)
    {
      this.destination = $(newDest).val();
    },
    createMvDialog: function () {
      var _this = this;
      this.mvDialog = $('<div id="mvDialog" tabindex="-1" role="dialog"></div>');
      this.mvDialog.addClass('modal');
      this.mvDialog.addClass('fade');
      var md = $('<div role="document"></div>');
      md.addClass('modal-dialog');
      var mdCnt = $('<div class="modal-content"></div>');
      mdCnt.addClass('modal-content');
      var mdHeader = $('<div class="modal-header"></div>');
      mdHeader.append('<h5 class="modal-title">Move files</h5>');
      mdHeader.append('<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>');
      mdCnt.append(mdHeader);
      var mdBody = $('<div class="modal-body"></div>');
      mdBody.append('<p>You´re about to move these files :</p>');
      this.flist = $('<ul></ul>');
      mdBody.append(this.flist);
      var inputDest = $('<input type="text" class="form-control" hint="dest/dir">').on('change', function() { _this.onDestinationChanged(this); });
      this.rootDisplay = $('<p></p>');
      this.rootDisplay.addClass('hidden');
      mdBody.append(this.rootDisplay);
      var rp = $('<p>to the folowing sub-directory </p>');
      mdBody.append(rp);
      inputDest.appendTo(mdBody);
      mdCnt.append(mdBody);
      var moveBtn = $('<button type="button" data-dismiss="modal" class="btn btn-primary">Do move these files</button>').click(function () {
        _this.moveSelectedFiles();
      });
      var mdFooter = $('<div class="modal-footer"></div>');
      mdFooter.append(moveBtn);
      mdFooter.append('<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>');
      mdCnt.append(mdFooter);
      md.append(mdCnt);
      this.mvDialog.append(md);
      this.mvDialog.appendTo(this.element);
    },
    _create: function () {
      var $view = this.element;
      var _this = this;
      this.dirBar = $('<div></div>');
      this.dirBar.appendTo($view);
      this.ftable = $('<table border="1">')
        .css('border-spacing', '6px')
        .css('border-collapse', 'separate');
      var btnRm = $('<button class="glyphicon">&#xe014;</button>').click(function () {
          _this.askForRemoval();
        });
      var btnMv = $('<button class="glyphicon">&#xe068;</button>').click(function () {
          _this.askForMoving();
        });
      var tr = $('<tr class="fileheaders"></tr>');
      _this.ftable.append(tr);
      tr.append($('<th></th>').append(btnRm).append(btnMv)).append('<th>Nom</th><th>Taille</th><th>Modification</th>');
      _this.ftable.appendTo($view);
      $('<div id="log">Logs<br/></div>').appendTo($view);
      this.createRmDialog();
      this.createMvDialog();
      this.openDir($view.data('path'));
    }
  });
})(window.jQuery);