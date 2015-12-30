<%@ Page Title="Post" Language="C#" Inherits="System.Web.Mvc.ViewPage<UUTBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title+ " - " + ViewData ["BlogTitle"] ; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">

<% if (!string.IsNullOrEmpty((string)ViewData["Avatar"])) { %>
<a href="<%= Url.Action("UserPosts", new{user=Model.Author}) %>" id="avatar">
<img src="<%=ViewData["Avatar"]%>" />
</a>
<% } %>
<h1 class="blogtitle">
<a href="<%= Url.RouteUrl("BlogByTitle", new{ user=Model.Author, title = Model.Title}) %>">
<%=Model.Title%>
</a>
 - <%= Html.TranslatedActionLink((string)ViewData ["BlogTitle"] ,"UserPosts",new{user=Model.Author}, null) %>
  - 
<a href="<%=Url.Content("~/")%>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (var be in Model) { %>
<div class="post">
<% if (be.Photo != null) { %>
	<img src="<%=Url.Content(be.Photo)%>" alt="<%=be.Title%>" class="photo">
<% } %>

<%= Html.Markdown(be.Content,"/bfiles/"+be.Id+"/") %>
 <%= Html.Partial("PostActions",be)%>
 <% string username = Membership.GetUser()==null ? null : Membership.GetUser().UserName; %>
		<% foreach (var c in (Comment[]) BlogManager.GetComments(be.Id)) {  %> 
<div class="comment" style="min-height:32px;"> 
<img class="avatar" src="<%= Url.RouteUrl("Default", new { action="Avatar", controller="Account", user = c.From } ) %>" alt="<%=c.From%>"/>
<%= Html.Markdown(c.CommentText) %>
	<% if (Model.Author ==  username || c.From == username ) { %>
	<%= Html.TranslatedActionLink("Supprimer","RemoveComment", new { cmtid = c.Id } , new { @class="actionlink" })%>
	<% } %>
</div><% } %>
</div><% } %>
</asp:Content>
