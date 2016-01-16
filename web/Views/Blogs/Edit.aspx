<%@ Page Title="Bill_edition" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" %>
<asp:Content ID="initContent" ContentPlaceHolderID="init" runat="server">
</asp:Content>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript" src="<%=Url.Content("~/Scripts/rangy-core.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/rangy-selectionsaverestore.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/jquery.htmlClean.min.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/hallo.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/to-markdown.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/showdown.js")%>"></script>
<link rel="stylesheet" type="text/css" href="/App_Themes/hallo.image.css">
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<aside>
	Id: 
	<a class="actionlink" href="<%= Url.RouteUrl("BlogById",new { action="UserPost", postid = Model.Id } ) %>">
	<%=Model.Id.ToString()%>
	</a>
, Posted: <%= Model.Posted.ToString("yyyy/MM/dd") %> - Modified: <%= Model.Modified.ToString("yyyy/MM/dd") %> 
Visible: <%= Model.Visible? "oui":"non" %> <%= Html.TranslatedActionLink("Supprimer","RemovePost", new { user=Model.Author, title = Model.Title, postid = Model.Id }, new { @class="actionlink" } ) %>
</aside>

<div id="damn"></div>
<form method="post" action="<%=Url.RouteUrl("BlogById",new {action="Edit",postid=Model.Id})%>" enctype="multipart/form-data" >
<a class="actionlink"> <h1 id="vtitle" for="Title" class="editable" ><%=Html.Markdown(Model.Title)%></h1></a>
<div id="vphoto" for="Photo" class="ronh">
<img src="<%=Model.Photo%>" alt="photo" class="photo"/>
<input type="file" name="PhotoUpload" id="PhotoUpload" class="control" >
</div>
<div id="vcontent" for="Content" class="editable ronh">
<%=Html.Markdown(Model.Content,"/bfiles/"+Model.Id+"/")%>
</div>
<div id="attachfiles" class="ronh">
<fieldset><legend><%= Html.Translate("AttachedFiles") %></legend>
<%= Html.FileList("~/bfiles/"+Model.Id) %>
<div class="control">
<input type="file" name="AttachedFiles" id="AttachedFiles" multiple>
</div>
</fieldset>
</div>
<%=Html.ValidationSummary()%>
<%= Html.Hidden("Id") %>
<%= Html.Label("Title") %>:
<%= Html.TextBox("Title") %>
<%= Html.ValidationMessage("Title", "*") %>
<%= Html.Label("Photo") %>:
<%= Html.TextBox("Photo") %>
<%= Html.ValidationMessage("Photo", "*") %>
<%= Html.Label("Content") %>:
<%= Html.TextBox("Content") %>
<%= Html.ValidationMessage("Content", "*") %>
<input type="submit" value="Create/Update" id="Submit"/>

</form>
<script>

$(document).ready(function(){
var opts={lang:'fr'};

jQuery('#vtitle').hallo({
  plugins: {
    'halloformat': opts,
    'halloreundo': opts,
  },
  toolbar: 'halloToolbarFixed',
  lang: 'fr'
});



jQuery('#vcontent').hallo({
  plugins: {
    'halloformat': opts,
    'halloheadings': opts,
    'hallolists': opts,
    'hallo-image-insert-edit': opts,
    'halloreundo': opts,
    'hallocleanhtml': {
            format: false,
            allowedTags: [
                'i',
                'p',
                'em',
                'strong',
                'br',
                'ol',
                'ul',
                'li',
                'a',
                'audio',
                'video',
                'img',
                'table',
                'tr',
                'td',
                'th',
                'style',
                ]
            },
    'halloblacklist': {tags: ['style']},
  },
  toolbar: 'halloToolbarFixed'
});


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
  return toMarkdown(html);
  };
 var htmlize = function(content) {
    return converter.makeHtml(content);
  };
  var updateHtml = function(id,content) {
  console.log("here"+id);
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
  jQuery('#vtitle').bind('hallomodified', onMDModified);
  jQuery('#vcontent').bind('hallomodified', onMDModified);
  jQuery('#vphoto').bind('hallomodified', onMDModified);
  jQuery('#Title').bind('change', function() { updateHtml(this.id,this.value); });
  jQuery('#Content').bind('change', function() { updateHtml(this.id,this.value); });

  });

</script>

</asp:Content>
