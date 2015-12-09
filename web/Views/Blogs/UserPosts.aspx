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

<% foreach (var g in Model.GroupByTitle()) { %>
<div class="panel">
<% foreach (var p in g) {  %> 
<div class="postpreview">
<a href="<%= Url.RouteUrl("Titles", new { title = g.Key}) %>" >
<% if (p.Photo!=null) { %>
<img src="<%=p.Photo%>" alt="<%=g.Key%>" >
<% } else { %> 
<%} %><%=Html.Encode(g.Key)%></a>
<div>
<%= Html.Markdown(p.Intro,"/bfiles/"+p.Id+"/") %>
 <%= Html.Partial("PostActions",p)%></div>
</div> <% break; } %>
</div>
 <% } %>
 <%= Html.RenderPageLinks((int) ViewData["PageIndex"],(int)ViewData["PageSize"],(int)ViewData["ResultCount"])%>
</asp:Content>
