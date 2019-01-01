var markdownize = function(content) {
  if (!content) return '';
    var html = content.split("\n").map($.trim).filter(function(line) {
      return line != "";
    }).join("\n");
  return toMarkdown(html);
  };

var converter = new showdown.Converter();

 var htmlize = function(content) {
    return converter.makeHtml(content);
  };

  var updateMD = function(id,content) {
  if (!content) return jQuery('#'+id).val('') ;
    var markdown = markdownize(content);
    if (jQuery('#'+id).val() === markdown) {
      return;
    }
    jQuery('#'+id).val( markdown );
  };
