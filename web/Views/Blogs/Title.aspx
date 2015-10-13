<%@ Page Title="Titre" Language="C#" Inherits="System.Web.Mvc.ViewPage<UTBlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title + " - " + YavscHelpers.SiteName; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1 class="post">
<%=Html.ActionLink(Model.Title, "Title", new{id=Model.Title}, null)%>
- <a href="<%= Url.Content("~/") %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%  foreach (BlogEntry e in this.Model) { %>
<div class="post<% if (!e.Visible) { %> hiddenpost<% } %>" >
<% if (e.Photo!=null) { %><img src="<%=e.Photo%>" alt="" class="photo"><% } %>
<%= Html.Markdown(e.Content,"/bfiles/"+e.Id+"/") %>
<%= Html.Partial("PostActions",e)%>
</div>
<% } %>
<%
if (((int) ViewData["RecordCount"]) > ((int) ViewData["PageSize"])) {
 rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.PageIndex = (int) ViewData["PageIndex"];
 rp1.PageSize = (int) ViewData["PageSize"];
%><aside><form runat="server" id="form1">
<yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server">
	 <None><i>Pas de contenu</i></None>
</yavsc:ResultPages> 
	</form>
</aside>
<% } %>
</asp:Content>
