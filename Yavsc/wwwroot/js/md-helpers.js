var markdownize = function(content) {
  if (!content) return '';
    var html = content.split("\n").map($.trim).filter(function(line) {
      return line != "";
    }).join("\n");
  return toMarkdown(html);
  };

var converter = new showdown.Converter();
var markdownize = function(content) {
  if (!content) return '';
    var html = content.split("\n").map($.trim).filter(function(line) {
      return line != "";
    }).join("\n");
    var md = toMarkdown(html);
    console.log(md);
  return md;
  };
 var htmlize = function(content) {
    return converter.makeHtml(content);
  };
  var updateHtml = function(id,content) {
    var jView = jQuery('*[for="'+id+'"]');
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
    $('#Submit').addClass('dirty');
    updateMD(this.attributes["for"].value, data.content);
  });
