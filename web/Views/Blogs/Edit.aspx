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


<% using(Html.BeginForm("ValidateEdit","Blogs")) { %>
<fieldset>
<legend>Contrôle d'accès au Billet</legend>
<%= Html.LabelFor(model => model.Visible) %> : <%= Html.CheckBox( "Visible" ) %> 
<i id="note_visible">Note: Si un ou plusieurs cercles sont séléctionnés ici,
 le billet ne sera visible qu'aux membres de ces cercles.</i>
<%= Html.ValidationMessage("Visible", "*") %>
<%= Html.LabelFor(model => model.AllowedCircles) %>
<%= Html.ListBox("AllowedCircles") %>
<%= Html.ValidationMessage("AllowedCircles", "*") %>
</fieldset>
<span id="viewsource">
<i class="fa fa-code menuitem"><%=Html.Translate("View_source")%></i></span>
<span id="hidesource" class="hidden">
<i class="fa fa-code menuitem"><%=Html.Translate("Hide_source")%></i>
</span>
<fieldset id="source" class="hidden">
<%=Html.Hidden("Author")%>
<%=Html.Hidden("Id")%>
<%= Html.LabelFor(model => model.Photo) %>
<%=Html.TextBox("Photo")%>
<%=Html.ValidationMessage("Photo")%><br>
<%= Html.LabelFor(model => model.Title) %>
<%=Html.TextBox("Title")%>
<%=Html.ValidationMessage("Title")%><br>
<%=Html.TextArea("Content")%>
<%=Html.ValidationMessage("Content")%>
</fieldset>
<input type="submit" id="validate" value="Valider" class="fa fa-check menuitem">
<% } %>

<form id="frmajax">
<fieldset>
<legend>Attacher des fichiers</legend>
<input type="file" name="attached" id="postedfile" multiple>
<input type="button" value="attacher les ficher" onclick="submitFile()">
<input type="button" value="importer les documents" onclick="submitImport()">
</fieldset>
</form>

<span class="placard editable" for="Photo">
<img src="<%=Model.Photo%>" alt="photo" id="vphoto" >
</span>
<!-- TODO? Model.Photo.(Legend|Date|Location|ref) -->
<h1 id="vtitle" for="Title" class="editable" ><%=Html.Markdown(Model.Title)%></h1>
<div id="vcontent" for="Content" class="editable">
<%=Html.Markdown(Model.Content,"/bfiles/"+Model.Id+"/")%>
</div>
<hr><h2>Fichiers attachées</h2> 

<%= Html.FileList("~/bfiles/"+Model.Id) %>

<hr>
<script>

$(document).ready(function(){

	$('#hidesource').click(function(){
	$('#source').addClass('hidden');
	$('#viewsource').removeClass('hidden');
	$('#hidesource').addClass('hidden');
	});
	$('#viewsource').click(function(){
	$('#source').removeClass('hidden');
	$('#viewsource').addClass('hidden');
	$('#hidesource').removeClass('hidden');
	});

jQuery('.placard').hallo({plugins: {'hallo-image-insert-edit': { lang: 'fr' } } });

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
	if (!content) return '';
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
	if (!content) content = '';
    var markdown = markdownize(content);
    if (jQuery('#'+id).val() === markdown) {
      return;
    }
    jQuery('#'+id).val( markdown );
  };

var updateHtml = function(id,content) {
  	var jView = jQuery('*[for="'+id+'"]');
    if (markdownize(jView.html()) === content) {
      return;
    }
    var html = htmlize(content);
    jView.html(html); 
  };

  jQuery('.placard').bind('hallomodified', function(event, data) {
   // TODO get image source from data.content
      $('#'+this.attributes["for"].value).val(
       $('#vphoto').attr('src'));
  });

  // Update Markdown every time content is modified
  var onMDModified = ( function (event, data) {
    showSource(this.attributes["for"].value, data.content);
  });
  jQuery('#vtitle').bind('hallomodified', onMDModified);
  jQuery('#vcontent').bind('hallomodified', onMDModified);

  jQuery('#Content').bind('keyup', function() {
    updateHtml(this.id, this.value);
  });
  jQuery('#Title').bind('keyup', function() {
    updateHtml(this.id, this.value);
  });

  // showSource("Title",jQuery('#vtitle').html());
  // showSource("Content",jQuery('#vcontent').html());
  });
</script>

	
<script>

function submitFilesTo(method)
{
	var data  = new FormData($('#frmajax').get()[0]);
	Yavsc.notice('Submitting via '+method);
	$.ajax({
	  url: apiBaseUrl+'/Blogs/'+method+'/'+$('#Id').val(),
	  type: "POST",
	  data: data,
	  processData: false, 
	  contentType: false,
	  success: function(data) {
	  $('#Content').val(data+"\n"+$('#Content').val());
	  Yavsc.notice(false);
	    },
	  error: Yavsc.onAjaxError,
	});
}

function submitImport()
{ submitFilesTo('Import'); }

function submitFile()
{ submitFilesTo('PostFile'); }

function submitBaseDoc()
{ 
var data  = new FormData($('#frmajax').get()[0]);
	Yavsc.notice('Submitting via '+method);
	$.ajax({
	  url: apiBaseUrl+'/Blogs/'+method+'/'+$('#Id').val(),
	  type: "POST",
	  data: data,
	  processData: false, 
	  contentType: false,
	  success: function(data) {
	  $('#Content').val(data+"\n"+$('#Content').val());
	  Yavsc.notice('Posted updated');
	    },
	  error: Yavsc.onAjaxError,
	});
}

</script>



<aside>
	Id:<%= Html.ActionLink( Model.Id.ToString() , "UserPost", new { user= Model.Author, title=Model.Title, id = Model.Id }, new { @class = "usertitleref actionlink" }) %>
, Posted: <%= Model.Posted.ToString("yyyy/MM/dd") %> - Modified: <%= Model.Modified.ToString("yyyy/MM/dd") %> 
Visible: <%= Model.Visible? "oui":"non" %> <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.Author, title = Model.Title, id = Model.Id }, new { @class="actionlink" } ) %>
</aside>

</asp:Content>