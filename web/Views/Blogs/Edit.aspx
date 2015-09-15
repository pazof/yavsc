<%@ Page Title="Bill_edition" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="head" ID="HeadContent1" runat="server">
<link rel="stylesheet" href="<%=Url.Content("~/Scripts/mdd_styles.css")%>">
 <script type="text/javascript" src="<%=Url.Content("~/Scripts/MarkdownDeepLib.min.js")%>">
  </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	 <% if (Model != null ) if (Model.Content != null )  { %>
	 <%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title,id=Model.Id}) %>

<% } %>

<%= Html.ValidationSummary("Edition du billet") %>

<% using(Html.BeginForm("ValidateEdit","Blogs")) { %>
<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.LabelFor(model => model.Content) %>:<br/>
<div class="mdd_toolbar"></div>
<%= Html.TextArea( "Content" , new { @class="mdd_editor"}) %>
<div class="mdd_resizer"></div>
<div class="mdd_preview panel"></div>

<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>

<%= Html.ValidationMessage("Visible", "*") %>
<br/>

<%= Html.LabelFor(model => model.AllowedCircles) %>
<%= Html.ListBox("AllowedCircles") %>

<%= Html.ValidationMessage("AllowedCircles", "*") %>
<%= Html.Hidden("Id") %>
<%= Html.Hidden("UserName") %>

<br/>
<input type="submit"/>
<% } %>


<script>
 $(document).ready(function () {
 $("textarea.mdd_editor").MarkdownDeep({ 
    help_location: "/Scripts/mdd_help.htm",
    disableTabHandling:false,
    ExtraMode: true
 });});

</script>
<% if (Model.Id!=0) {  %> 
<script>


function submitTo(method)
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
	submitTo('Import');
}

function submitFile()
{
	submitTo('PostFile');
}
</script>
<form id="uploads" method="post" enctype="multipart/form-data">
<input type="file" name="attached" id="postedfile" multiple>
<input type="button" value="attacher les ficher" onclick="submitFile()">
<input type="button" value="importer les documents" onclick="submitImport()">
</form>


<% } %> 
</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="MASContentContent" runat="server">
<div class="metablog">
	(Id:<a href="/Blogs/UserPost/<%= Model.Id %>">
<i><%= Model.Id %></i></a>, <%= Model.Posted.ToString("yyyy/MM/dd") %> - <%= Model.Modified.ToString("yyyy/MM/dd") %> <%= Model.Visible? "":", Invisible!" %>) <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.UserName, title = Model.Title }, new { @class="actionlink" } ) %>
</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="masContent1" runat="server">

</asp:Content>
