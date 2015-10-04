<%@ Page Title="Blogs - Index" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<% foreach (var g in Model.GroupByUser()) { %>
<h2><a href="<%= Url.Content("~/Blog/"+g.Key) %>" class="actionlink userref">
<%=g.Key%></a></h2>
<% foreach (var p in g) { %> 
<div class="postpreview">
<%= Html.ActionLink(p.Title, "UserPost",
 new { user = g.Key, title = p.Title }, new { @class = "usertitleref" } ) %>
 <p><%=  Html.Markdown(p.Intro,"/bfiles/"+p.Id+"/") %></p>
  <aside>
(Posté le  <%=p.Posted.ToString("D") %>)

  <% if (Membership.GetUser()!=null)
	if ((Membership.GetUser().UserName==g.Key)
	|| (Roles.IsUserInRole ("Admin")))
	 { %>
	 <%= Html.ActionLink("Editer","Edit", new { id = p.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = p.Id }, new { @class="actionlink" } ) %>
<% } %> </aside>
</div> <% } %>
 <% } %>
</div>
<form runat="server" id="form1" method="GET">
<% rp1.ResultCount =  (int) ViewData["ResultCount"]; 
   rp1.PageSize = (int) ViewData ["PageSize"]; 
   rp1.PageIndex = (int) ViewData["PageIndex"]; 
   rp1.None = Html.Translate("no content"); 
%>
	 	 	 <yavsc:ResultPages id="rp1" runat="server" >
	 <None>Aucun résultat</None>
	 </yavsc:ResultPages> 
</form>
</asp:Content>

