if (typeof jQuery === 'undefined') {
    throw new Error('Bootstrap\'s JavaScript requires jQuery')
}

+
(function($) {
    $.widget("psc.blogcomment", {
        options: {
            apictrlr: null,
            omob: '#ffe08030',
            omof: '#501208',
            bgc: '#fff',
            fgc: '#000',
            lang: 'fr-FR'
        },
        editable: false,
        editting: false,
        edctrl: null,
        _create: function() {
            var _this = this;
            this.element.addClass("blogcomment");
            var date = new Date(this.element.data("date"));
            var username = this.element.data("username");
            this.editable = this.element.data("allow-edit");
            this.element.prepend('<div class="commentmeta"><div class="avatar"><img src="/Avatars/' + username + '.xs.png" class="smalltofhol" />' + username + '</div><div class="cmtdatetime">' +
                date.toLocaleDateString(this.options.lang) + ' ' + date.toLocaleTimeString(this.options.lang) + '</div></div>')
            this.element.on("mouseenter", this.onMouseEnter);
            this.element.on("mouseleave", this.onMouseLeave);
            if (this.editable) {
                this.element.on("click", function(ev) { _this.toggleEdit(_this, ev) })
            }
        },
        toggleEdit: function(_this, ev) {
            if (!_this.edctrl) {
                // TODO click on button
                _this.edctrl = $("<button class=\"btn btn-default\">Delete</button>");
                _this.edctrl.on("click", function(ev) { _this.doDeleteComment(_this, ev) }).appendTo(_this.element);
            } else {
                _this.edctrl.remove();
                _this.edctrl = null;
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
        }

    });

    $(document).ready(function() {
        $("[data-type='blogcomment']").blogcomment();
    })
})(jQuery);