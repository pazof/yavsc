var markdownize = function(content) {
  if (!content) return '';
    var html = content.split('\n').map($.trim).filter(function(line) {
      return line != '';
    }).join('\n');
  return toMarkdown(html);
  };

var converter = new showdown.Converter();

var htmlize = function(content) {
    return converter.makeHtml(content);
  };

var updateMD = function(id,content) {
  if (!content) return jQuery('#' + id).val('');
    var markdown = markdownize(content);
    if (jQuery('#' + id).val() === markdown) {
      return;
    }
    jQuery('#' + id).val(markdown);
  };

var allowDrop = function (ev) {
    ev.preventDefault();
}

var drag = function (ev) {
    ev.dataTransfer.setData('text', ev.target.id);
}

var drop  = function (ev) {
    ev.preventDefault();
    var data = ev.dataTransfer.getData('text');
    ev.target.appendChild(document.getElementById(data));
}
