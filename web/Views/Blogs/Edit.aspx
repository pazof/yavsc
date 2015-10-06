<%@ Page Title="Bill_edition" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="head" ID="HeadContent1" runat="server">

<script type="text/javascript" src="<%=Url.Content("~/Scripts/rangy-core.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/rangy-selectionsaverestore.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/jquery.htmlClean.min.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/hallo.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/to-markdown.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/showdown.js")%>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<% using(Html.BeginForm("ValidateEdit","Blogs")) { %>
<fieldset>
<legend>Billet</legend>
<%= Html.LabelFor(model => model.Title) %> <%= Html.ValidationMessage("Title") %> : <br>
<input name="Title" id="Title" class="fullwidth">
<br>
<%= Html.LabelFor(model => model.Content) %> 
<%= Html.ValidationMessage("Content") %>: <br>
<style> #Content { } 
</style>
<textarea id="Content" name="Content" class="fullwidth" ><%=Html.Markdown(Model.Content)%></textarea><br>

<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>

<%= Html.ValidationMessage("Visible", "*") %>
<br/>

<%= Html.LabelFor(model => model.AllowedCircles) %>
<%= Html.ListBox("AllowedCircles") %>

<%= Html.ValidationMessage("AllowedCircles", "*") %>

<%=Html.Hidden("Author")%>
<%=Html.Hidden("Id")%>
<input type="submit">
</fieldset>
<% } %>
</div>
<div class="post">
<h1><div id="vtitle" for="Title" class="post title editable"><%=Html.Markdown(Model.Title)%></div></h1>
<div id="vcontent" for="Content" class="post content editable">
<%=Html.Markdown(Model.Content,"/bfiles/"+Model.Id+"/")%>
</div>
</div>


<script>
jQuery('#vtitle').hallo({
  plugins: {
    'halloformat': {},
      'halloreundo': {}
  },
  toolbar: 'halloToolbarFixed',
  lang: 'fr'
});

jQuery('#vcontent').hallo({
  plugins: {
    'halloformat': {},
    'halloheadings': {},
    'hallolists': {},
    'hallo-image-insert-edit': {
      lang: 'fr'
    },
    'halloimage': {
      searchUrl: apiBaseUrl+'/Blogs/SearchFile/'+$('#Id').val(),
      uploadUrl: apiBaseUrl+'/Blogs/PostFile/'+$('#Id').val(),
      suggestions: true,
      insert_file_dialog_ui_url: '<%= Url.Content("~/Blog/ChooseMedia/?id="+Model.Id) %>'
    },
    'halloreundo': {},
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
                'style'
                ]
            },
    'halloblacklist': {tags: ['style']},
  },
  toolbar: 'halloToolbarFixed'
});

var markdownize = function(content) {
    var html = content.split("\n").map($.trim).filter(function(line) { 
      return line != "";
    }).join("\n");
  return toMarkdown(html);
  };

var converter = new showdown.Converter(),
    htmlize = function(content) {
    return converter.makeHtml(content);
  };

   // Method that converts the HTML contents to Markdown
var showSource = function(id,content) {
    var markdown = markdownize(content);
    if (jQuery('#'+id).get(0).value == markdown) {
      return;
    }
    jQuery('#'+id).get(0).value = markdown;
  };

var updateHtml = function(id,content) {
  	var jView = jQuery('div[for="'+id+'"]');
    if (markdownize(jView.html()) == content) {
      return;
    }
    var html = htmlize(content);
    jView.html(html); 
  };

  // Update Markdown every time content is modified
  jQuery('.editable').bind('hallomodified', function(event, data) {
    showSource(this.attributes["for"].value, data.content);
  });
  jQuery('#Content').bind('keyup', function() {
    updateHtml(this.id, this.value);
  });
  jQuery('#Title').bind('keyup', function() {
    updateHtml(this.id, this.value);
  });

  showSource("Title",jQuery('#vtitle').html());
  showSource("Content",jQuery('#vcontent').html());

</script>

	
<script>

function submitFilesTo(method)
{
	var data  = new FormData($('#uploads').get()[0]);
	Yavsc.message('Submitting via '+method);
	$.ajax({
	  url: apiBaseUrl+'/Blogs/'+method+'/'+$('#Id').val(),
	  type: "POST",
	  data: data,
	  processData: false, 
	  contentType: false,
	  success: function(data) {
	  $('#Content').val(data+"\n"+$('#Content').val());
	  Yavsc.message(false);
	    },
	  error: Yavsc.onAjaxError,
	});
}

function submitImport()
{ submitFilesTo('Import'); }

function submitFile()
{ submitFilesTo('PostFile'); }
</script>
<form id="uploads" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Fichiers attachés</legend>
<input type="file" name="attached" id="postedfile" multiple>
<input type="button" value="attacher les ficher" onclick="submitFile()">
<input type="button" value="importer les documents" onclick="submitImport()">
</fieldset>
</form>

<aside>
	Id:<%= Html.ActionLink( Model.Id.ToString() , "UserPost", new { user= Model.Author, title=Model.Title, id = Model.Id }, new { @class = "usertitleref actionlink" }) %>
, Posted: <%= Model.Posted.ToString("yyyy/MM/dd") %> - Modified: <%= Model.Modified.ToString("yyyy/MM/dd") %> 
Visible: <%= Model.Visible? "oui":"non" %> <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.Author, title = Model.Title }, new { @class="actionlink" } ) %>
</aside>

</asp:Content>