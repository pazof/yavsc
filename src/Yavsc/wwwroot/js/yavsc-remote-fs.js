// requires DropZone ª toMarkdown

if (typeof window.jQuery === 'undefined') {
  throw new Error('yavsc-remote-fs script requires jQuery');
}

(function ($) {
        $.widget('psc.yarfs', {
            options: {
                fsnurl: '/api/fs'
            },
            root: null,
            rmAlert: null,
            flist: null,
            selection: [],
            dirBar: null,
            openDir: function (sub) {
                var _this = this;
                this.root = sub;
                var owner = this.element.data('owner');
                this.selection = [];
                this.dirBar.empty();
                $('<button>' + owner + '</button>').click(function() {
                    _this.openDir(null);
                }).appendTo(this.dirBar);
                var npath = null;

                if (_this.root) {
                    var dnames = _this.root.split('/');
                    $.each(dnames, function () {
                        var part = this;
                        if (npath == null) npath = part;
                        else npath = npath + '/' + part;
                        $('<button/>').append(part).click(function() {
                            _this.OpenDir(npath);
                        }).appendTo(this.dirBar);
                    });
                }

                this.ftable.find('tr.fileentry').remove();
                var fsiourl = this.root ? '/api/fs/' + this.root : '/api/fs';
                $.get(fsiourl, function(data) {
                    $.each(data.SubDirectories, function () {
                        var item = this;
                        var spath = (_this.root) ? _this.root + '/' + item.Name : item.Name;
                        $('<button/>').append(item.Name).click(function() {
                            _this.openDir(spath);
                        }).appendTo(_this.dirBar);
                    });

                    $.each(data.Files, function () {
                        var item = this;
                        var $tr = $('<tr class="fileentry"></tr>');
                        var $td = $('<td></td>');
                        $td.appendTo($tr);
                        $('<input type="checkbox" />').addClass('check-box').click(function() {
                            _this.SetItemSelected(item.Name, this.checked);
                        }).appendTo($td);

                        $('<td></td>').append($('<a></a>').append(item.Name).click(function() {
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
                if (selected) this.selection.push(name);
                else this.selection = this.selection.filter(function(ele) {
                    return ele != name;
                });
            },
            RemoveSelectedFiles: function () {
                $.each(this.selection, function() {
                    var xmlhttp = new XMLHttpRequest();
                    xmlhttp.open('DELETE', '/api/fs/' + this, true);
                    xmlhttp.send();
                });
                this.selection = [];
                // FIXME this could fail for a very long list of big files
                setTimeout(500, function() { this.openDir(this.root); });
            },
            askForRemoval: function () {
                this.flist.empty();
                var _this = this;
                $.each(this.selection, function () {
                    _this.flist.append('<li>' + this + '</li>');
                });
                this.rmAlert.modal({ show: true });
            },
            _create: function () {
                var $view = this.element;
                var _this = this;
                this.dirBar = $('<div></div>');
                this.dirBar.appendTo($view);
                this.ftable = $('<table border="1">').css('border-spacing', '6px')
                        .css('border-collapse', 'separate');
                this.openDir($view.data('path'));
                var btnRm = $('<button class="glyphicon">&#xe083;</button>').click(function() { _this.askForRemoval(); });
                var tr = $('<tr class="fileheaders"></tr>');
                _this.ftable.append(tr);
                tr.append($('<th></th>').append(btnRm)).append('<th>Nom</th><th>Taille</th><th>Modification</th>');
                _this.ftable.appendTo($view);
                this.rmAlert = $('<div id="rmAlert" tabindex="-1" role="dialog"></div>');
                this.rmAlert.addClass('modal');
                this.rmAlert.addClass('fade');
                var md = $('<div role="document"></div>');
                md.addClass('modal-dialog');
                this.rmAlert.append(md);
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
                var rmcBtn = $('<button type="button" data-dismiss="modal" class="btn btn-primary">Do deletion</button>')
                    .click(function() { _this.RemoveSelectedFiles(); });
                var mdFooter = $('<div class="modal-footer"></div>');
                mdFooter.append(rmcBtn);
                mdFooter.append('<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>');
                mdCnt.append(mdFooter);
                md.append(mdCnt);
                this.rmAlert.appendTo($view);
            }
         });

})(window.jQuery);

