$.widget("psc.blogcomment", {
        options: {
            apictrlr: null,
            authorId: null,
            authorName: null,
            omob: '#ffe08030',
            omof: '#501208',
            bgc: '#fff',
            fgc: '#000',
            lang: 'fr-FR',
            allowCoc: true
        },
        editable: false,
        editting: false,
        hideBtn: null,
        delBtn: null,
        cmtInput: null,
        cmtBtn: null,
        ctlBtn: null,
        collapsed: false,
        subCmts: null,
        _create: function() {
            var _this = this;
            this.element.addClass("blogcomment");
            var date = new Date(this.element.data("date"));
            var username = this.element.data("username");
            this.editable = this.element.data("allow-edit");
            this.element.prepend('<div class="commentmeta"><img class="avatar" src="/Avatars/' + username + '.xs.png" class="smalltofhol" title="' + username + '" /><div class="cmtdatetime">' +
                date.toLocaleDateString(this.options.lang) + ' ' + date.toLocaleTimeString(this.options.lang) + '</div></div>')
            this.element.on("mouseenter", this.onMouseEnter);
            this.element.on("mouseleave", this.onMouseLeave);

            this.ctlBtn = $('<button class="btn"><span class="ui-icon ui-icon-plus"></span></button>').on("click", function(ev) { _this.toggleCollapse(_this, ev) }).appendTo(_this.element);
        },
        toggleCollapse: function(_this, ev) {
            _this.collapsed = !_this.collapsed;
            if (_this.collapsed) {
                $(_this.ctlBtn).removeClass('glyphicon-collapse-down');
                $(_this.ctlBtn).addClass('glyphicon-collapse-up');
            } else {
                $(_this.ctlBtn).removeClass('glyphicon-collapse-up');
                $(_this.ctlBtn).addClass('glyphicon-collapse-down');
            }
            if (_this.editable) {
                _this.toggleEdit(_this, ev)
            }
            if (_this.options.allowCoc) {
                _this.toggleComment(_this, ev)
            }
        },
        toggleEdit: function(_this, ev) {
            if (!_this.delBtn) {
                _this.delBtn = $("<button class=\"btn btn-warning\">Supprimer</button>");
                _this.delBtn.on("click", function(ev) { _this.doDeleteComment(_this, ev) }).appendTo(_this.element);
            } else {
                _this.delBtn.remove();
                _this.delBtn = null;
            }
        },
        toggleComment: function(_this, ev) {
            if (!_this.cmtBtn) {
                if (!_this.subCmts) {
                    _this.subCmts = $(_this.element).children('div.subcomments');
                    if (_this.subCmts.length == 0) {
                        _this.subCmts = $('<div></div>').addClass('subcomments');
                        _this.subCmts.appendTo(_this.element);
                    }
                }
                _this.cmtInput = $('<input type="text" placeholder="Votre réponse"/>');
                _this.cmtInput.appendTo(_this.element);
                _this.cmtBtn = $("<button class=\"btn btn-default\">Répondre</button>");
                _this.cmtBtn.on("click", function(ev) { _this.doCoC(_this, ev) }).appendTo(_this.element);
            } else {
                _this.cmtInput.remove();
                _this.cmtBtn.remove();
                _this.cmtBtn = null;
            }
        },
        onMouseEnter: function() {
            $(this).animate({
                backgroundColor: $.psc.blogcomment.prototype.options.omob,
                color: $.psc.blogcomment.prototype.options.omof
            }, 400);
        },
        onMouseLeave: function() {
            $(this).animate({
                backgroundColor: $.psc.blogcomment.prototype.options.bgc,
                color: $.psc.blogcomment.prototype.options.fgc
            }, 400);
        },
        doDeleteComment: function(_this, ev) {
            var cmtid = $(_this.element).data("id");
            var cmtapi = _this.options.apictrlr;
            $.ajax({
                async: true,
                cache: false,
                type: 'POST',
                method: 'DELETE',
                error: function(xhr, data) {
                    $('span.field-validation-valid[data-valmsg-for="Content"]').html(
                        "Une erreur est survenue : " + xhr.status + "<br/>"
                    ).focus()
                },
                success: function(data) {
                    _this.element.remove()
                },
                url: cmtapi + '/' + cmtid
            });
        },
        doCoC: function(_this, ev) {
            var postid = $('#cmtBtn').data('receiverid');
            var comment = _this.cmtInput.val();
            var cmtid = $(_this.element).data("id");
            var data = {
                Content: comment,
                PostId: postid,
                ParentId: cmtid,
                AuthorId: _this.options.authorId
            };

            $.ajax({
                async: true,
                cache: false,
                type: 'POST',
                method: 'POST',
                contentType: "application/json",
                data: JSON.stringify(data),
                error: function(xhr, erd) {
                    console.log('err');
                    console.log(xhr);
                    console.log(erd);
                    $('span.field-validation-valid[data-valmsg-for="Content"]').html(
                        "Une erreur est survenue : " + xhr.status + "<br/>" +
                        "<code><pre>" + xhr.responseText + "</pre></code>"
                    )
                },
                success: function(data) {
                    var comment = data.Content;
                    _this.cmtInput.val('');
                    $('span.field-validation-valid[data-valmsg-for="Content"]').empty();
                    var htmlcmt = htmlize(comment);
                    $('<div data-type="blogcomment" data-id="' + data.Id + '" data-allow-edit="True" data-date="' + data.DateCreated + '" data-username="' + _this.options.authorName + '">' + htmlcmt + '</div>')
                        .blogcomment().appendTo(_this.subCmts);
                },
                url: _this.options.apictrlr
            });

        }

    });

    $(document).ready(function() {
        $("[data-type='blogcomment']").blogcomment();
    })

