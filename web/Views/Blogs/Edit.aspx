<%@ Page Title="Bill edition" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="HeadContent1" runat="server">
<link rel="stylesheet" href="<%=Url.Content("~/Theme/mdd_styles.css")%>">
 <script type="text/javascript" src="<%=Url.Content("~/Scripts/MarkdownDeepLib.min.js")%>">
 </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	 <% if (Model != null ) if (Model.Content != null )  { %>
	 <%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title}) %>

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

<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>
<%= Html.ValidationMessage("Visible", "*") %>
	<%= Html.Hidden("Id") %>
	<%= Html.Hidden("UserName") %>

<br/>
<input type="submit"/>
<% } %>

<script>
 $(document).ready(function () {
 $("textarea.mdd_editor").MarkdownDeep({ 
    help_location: "/Scripts/html/mdd_help.htm",
    disableTabHandling:false
 });});
</script>

</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="MASContentContent" runat="server">
<div class="metablog">
	(Id:<a href="/Blogs/UserPost/<%= Model.Id %>">
<i><%= Model.Id%></i></a>, <%= Model.Posted.ToString("yyyy/MM/dd") %> - <%= Model.Modified.ToString("yyyy/MM/dd") %> <%= Model.Visible? "":", Invisible!" %>) <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.UserName, title = Model.Title }, new { @class="actionlink" } ) %>
</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MASContent" ID="masContent1" runat="server">

<div class="mdd_preview panel"></div>
</asp:Content>
