<%@ Page Title="Bill_edition" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="head" ID="HeadContent1" runat="server">
<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
<script src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
<script src="http://rangy.googlecode.com/svn/trunk/currentrelease/rangy-core.js"></script>
<link rel="stylesheet" href="<%=Url.Content("~/Scripts/mdd_styles.css")%>">
<script type="text/javascript" src="<%=Url.Content("~/Scripts/MarkdownDeepLib.min.js")%>"></script>
<link rel="stylesheet" href="<%=Url.Content("~/App_Themes/jquery-ui.css")%>" />
<link rel="stylesheet" href="<%=Url.Content("~/App_Themes/font-awesome.css")%>" />
<script type="text/javascript" src="<%=Url.Content("~/Scripts/hallo.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/to-markdown.js")%>"></script>
<script type="text/javascript" src="<%=Url.Content("~/Scripts/showdown.js")%>"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<h1><div id="vtitle" for="Title" class="post title editable"><%=Html.Markdown(Model.Title)%></div></h1>
<div id="vcontent" for="Content" class="post content editable">
<%=Html.Markdown(Model.Content,"/bfiles/"+Model.Id+"/")%>
</div>
<% using(Html.BeginForm("ValidateEdit","Blogs")) { %>
<%= Html.LabelFor(model => model.Title) %> <%= Html.ValidationMessage("Title") %> : <br>
<input name="Title" id="Title">
<br>
<%= Html.LabelFor(model => model.Content) %> 
<%= Html.ValidationMessage("Content") %>: <br>
<textarea id="Content" name="Content"><%=Html.Markdown(Model.Content)%></textarea><br>
<%=Html.Hidden("Author")%>
<%=Html.Hidden("Id")%>
<input type="submit">
<% } %>
<script>
jQuery('#vtitle').hallo({
  plugins: {
    'halloformat': {},
      'halloreundo': {}
  },
  toolbar: 'halloToolbarFixed'
});
jQuery('#vcontent').hallo({
  plugins: {
    'halloformat': {},
      'halloheadings': {},
      'hallolists': {},
      'halloimage': {},
      'halloreundo': {}
  },
  toolbar: 'halloToolbarFixed'
});

var markdownize = function(content) {
    var html = content.split("\n").map($.trim).filter(function(line) { 
      return line != "";
    }).join("\n");
    return toMarkdown(html);
  };
  var converter = new Showdown.converter();
  var htmlize = function(content) {
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
{
	submitFilesTo('Import');
}

function submitFile()
{
	submitFilesTo('PostFile');
}

</script>
<form id="uploads" method="post" enctype="multipart/form-data">
<input type="file" name="attached" id="postedfile" multiple>
<input type="button" value="attacher les ficher" onclick="submitFile()">
<input type="button" value="importer les documents" onclick="submitImport()">
</form>
</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="MASContentContent" runat="server">
<div class="metablog">
	(Id:<a href="/Blogs/UserPost/<%= Model.Id %>">
<i><%= Model.Id %></i></a>, <%= Model.Posted.ToString("yyyy/MM/dd") %> - <%= Model.Modified.ToString("yyyy/MM/dd") %> <%= Model.Visible? "":", Invisible!" %>) <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.Author, title = Model.Title }, new { @class="actionlink" } ) %>
</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="masContent1" runat="server">

</asp:Content>
