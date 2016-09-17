var markdownize = function(content) {
  if (!content) return '';
    var html = content.split("\n").map($.trim).filter(function(line) {
      return line != "" ;
    }).join("\n");
  return toMarkdown(html);
  };

var htmlize = function(content) {
    return converter.makeHtml(content);
  };
  var updateHtml = function(jView,content) {
    if (markdownize(jView.html()) === content) {
      return;
    }
    var html = htmlize(content);
    jView.html(html);
  };

var updateMD = function(id,content) {
  if (!content) return jQuery('#'+id).val('') ;
    var markdown = markdownize(content);
    if (jQuery('#'+id).val() === markdown) {
      return;
    }
    jQuery('#'+id).val( markdown );
  };
var onMDModified = ( function (event, data) {
    $('#Submit').addClass('success');
    updateMD(this.attributes["for"].value, data.content);
  });
