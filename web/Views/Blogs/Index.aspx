<%@ Page Title="Blogs" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<% foreach (var g in Model.GroupByTitle()) { %>
<div class="postpreview panel">
<h2><a href="<%= Url.RouteUrl("Titles", new { title = g.Key }) %>" class="usertitleref"><%=Html.Encode(g.Key)%></a></h2>
<% foreach (var p in g) { %> 
<div>
<% if (p.Photo != null ) { %>
<img src="<%=p.Photo%>" alt="<%=p.Photo%>" class="photo">
<% } else {}  %>
<%=  Html.Markdown(p.Intro,"/bfiles/"+p.Id+"/") %>
 <%= Html.Partial("PostActions",p) %>
</div><% // we only show the first. It's a preview
 break;
 } %>
</div>
 <% } %>
</div>
 <%= Html.RenderPageLinks((int)ViewData["PageIndex"],(int)ViewData["PageSize"],(int)ViewData["ResultCount"])%>
</asp:Content>
