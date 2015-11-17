<%@ Page Title="Articles" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<% foreach (var g in Model.GroupByTitle()) { %>
<div class="panel">
<h2><a href="<%= Url.RouteUrl("Titles", new { title = g.Key }) %>" class="usertitleref"><%=Html.Encode(g.Key)%></a></h2>
<% foreach (var p in g) { %> 
<div class="postpreview">
 <p><%=  Html.Markdown(p.Intro,"/bfiles/"+p.Id+"/") %></p>
 <%= Html.Partial("PostActions",p)%>
</div> <% } %>
</div>
 <% } %>
</div>
 <%= Html.RenderPageLinks((int)ViewData["PageIndex"],(int)ViewData["PageSize"],(int)ViewData["ResultCount"])%>
</asp:Content>
