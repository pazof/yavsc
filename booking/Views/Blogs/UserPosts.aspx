<%@ Page Title="Blog" Language="C#" Inherits="System.Web.Mvc.ViewPage<UUBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = (string) ViewData ["BlogTitle"] ; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">

<% if (!string.IsNullOrEmpty((string)ViewData["Avatar"])) { %>
<a href="<%=Url.RouteUrl( "Blogs", new { user = Model.Author } )%>" id="avatar">
<img src="<%=ViewData["Avatar"]%>" />
</a>
<% } %>
<h1 class="blogtitle">
<a href="<%=Url.RouteUrl( "Blogs", new { user = Model.Author } )%>">
<%=Html.Encode(ViewData["BlogTitle"])%></a>
- <a href="<%= Url.RouteUrl( "Default", new { controller = "Home" } ) %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%  foreach (BlogEntry e in this.Model) { %>
<div class="postpreview<% if (!e.Visible) { %> hiddenpost<% } %>" >
<h2><a href="<%= Url.RouteUrl("BlogByTitle", new { user=e.Author, title=e.Title, postid = e.Id })%>" class="usertitleref">
<%=Html.Markdown(e.Title)%></a></h2>
<% bool truncated = false; %>
<%= Html.MarkdownToHtmlIntro(out truncated, e.Content,"/bfiles/"+e.Id+"/") %>
<% if (truncated) { %>
<a href="<%= Url.RouteUrl( "BlogByTitle", new { user=e.Author ,  title=e.Title, postid = e.Id}) %>">
  <i><%=Html.Translate("ReadMore")%></i></a>
  <% } %>
<%= Html.Partial("PostActions",e)%>
</div>
<% } %>
<aside>
	<form runat="server" id="form1" method="GET">
<%
 rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.PageIndex = (int) ViewData["PageIndex"];
%>
<yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server">
	 <None><i>Pas de contenu</i></None>
</yavsc:ResultPages> 
	</form>
</aside>
	

</asp:Content>
