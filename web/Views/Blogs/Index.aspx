<%@ Page Title="Articles" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<% foreach (var g in Model.GroupByTitle()) { %>
<h2><%=Html.ActionLink(g.Key, "Title", "Blogs", new { id = g.Key } , new { @class="userref" } )%></h2>
<% foreach (var p in g) { %> 
<div class="postpreview">
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
<%
 rp1.ResultCount =  (int) ViewData["ResultCount"]; 
   rp1.PageSize = (int) ViewData ["PageSize"]; 
   rp1.PageIndex = (int) ViewData["PageIndex"]; 
   rp1.None = Html.Translate("no content"); 
%>
	 	 	 <yavsc:ResultPages id="rp1" runat="server" >
	 <None>Aucun résultat</None>
	 </yavsc:ResultPages> 
</form>
</asp:Content>

