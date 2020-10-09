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
    fmlist: null,
    selection: [],
    curDirBar: null,
    subDirsBar: null,
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
      this.curDirBar.empty();
      this.subDirsBar.empty();
      $('<button>' + owner + '</button>')
        .click(function () {
          _this.openDir(null);
        })
        .appendTo(this.curDirBar);
      var npath = null

      if (_this.root) {
        var dnames = _this.root.split('/');
        $.each(dnames, function () {
          var part = this;
          if (npath == null) npath = encodeURIComponent(part);
          else npath = npath + '/' + encodeURIComponent(part);
          $('<button/>')
            .append(part)
            .click(function () {
              _this.openDir(npath);
            })
            .appendTo(_this.curDirBar);
        });
      }


      this.ftable.find('tr.fileinfo').remove();
      var fsiourl = this.root ? '/api/fs/' + this.root : '/api/fs';
      $.get(fsiourl, function (data) {
        if (data.SubDirectories.length == 0 && data.Files.length == 0)
        {
          $('<button class="glyphicon">&#xe014; remove this empty directory</button>').click(
            function() {
              var xmlhttp = new XMLHttpRequest();
              xmlhttp.open('DELETE', '/api/fs/' + _this.root, true);
              xmlhttp.send();

              xmlhttp.onreadystatechange = function(event) {
                // XMLHttpRequest.DONE === 4
                if (this.readyState === XMLHttpRequest.DONE) {
                  if (this.status === 200) {
                    var dnames = _this.root.split('/');
                    var dcnt = dnames.length;
                    var nroot = dnames.slice(0,dcnt-1).join('/');
                    _this.openDir(nroot);
                  }
                }
              }
            }).appendTo(_this.subDirsBar);
        }
        else 
        {
          $.each(data.SubDirectories, function () {
            var item = this;
            var spath = _this.root ? _this.root + '/' + encodeURIComponent(item.Name) : encodeURIComponent(item.Name);
            $('<button/>')
              .append(item.Name)
              .click(function () {
                _this.openDir(spath);
              })
              .appendTo(_this.subDirsBar);
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
              var furl =  (_this.root) ? '/files/' + owner + '/' + _this.root + '/' + encodeURIComponent(item.Name)
                : '/files/' + owner + '/' + encodeURIComponent(item.Name);
            $('<td class="filename"></td>')
              .append($('<a></a>').attr('href',furl)
                .append(item.Name)).appendTo($tr);
            $('<td class="filesize">' + item.Size + '</td>').appendTo($tr);
            $('<td class="filemdate">' + item.LastModified + '</td>').appendTo($tr);
            $tr.appendTo(_this.ftable);
          });
        }
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
    setFileAway: function(fname)
    {
      this.selection = this.selection.filter(function (ele) {
        return ele !== fname;
      });
      this.ftable.find('tr.fileinfo').filter(function () {
        return $(this).children('td:nth-child(2)').text() == fname;
      }).remove();
    },
    RemoveSelectedFiles: function () {
      var _this = this;
      $.each(this.selection, function () {
        var dfile = this;
        var dfilep = _this.root ? _this.root + '/' + this : this; 
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function(event) {
          if (this.readyState === XMLHttpRequest.DONE) {
            if (this.status === 200) {
              _this.setFileAway(dfile);
            }
            else {
              alert( 'deletion of : ' + dfile + 'failed : '+ this.statusText );
            }
          }
        };
        xmlhttp.open('DELETE', '/api/fs/' + encodeURIComponent(dfilep), true);
        xmlhttp.send();
      });
    },
    moveSelectedFiles: function () {
      var _this = this;
      var dest = this.destination;
      $.each(this.selection, function () {
        var mfile = this;
        var data = {};
        data['id'] = _this.root ? _this.root + '/' + mfile : mfile;
        data['to'] = dest;
        var request = $.ajax({
          url: '/api/fsc/mvftd',
          type: 'POST',
          data: JSON.stringify(data),
          contentType: 'application/json;charset=utf-8'
        });

        request.done(function() {
          _this.setFileAway(mfile);
        });
         
        request.fail(function( jqXHR, textStatus, msg ) {
          alert( 'Failed to move : ' + mfile + ' : ' + textStatus );
        });
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
      this.fmlist.empty();
      var _this = this;
      $.each(this.selection, function () {
        _this.fmlist.append('<li>' + this + '</li>');
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
      this.fmlist = $('<ul></ul>');
      mdBody.append(this.fmlist);
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
      this.curDirBar = $('<div class="curdir"></div>');
      this.curDirBar.appendTo($view);
      this.subDirsBar = $('<div class="subdirs"></div>');
      this.subDirsBar.appendTo($view);
      this.ftable = $('<table>')
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
      this.createRmDialog();
      this.createMvDialog();
      this.openDir($view.data('path'));
    }
  });
})(window.jQuery);
