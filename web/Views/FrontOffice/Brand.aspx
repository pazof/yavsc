<%@ Page Title="Catalog" Language="C#"  Inherits="System.Web.Mvc.ViewPage<Brand>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
  <h1><% if (Model.Logo!=null) { %>
  <img src="<%=Model.Logo.Src%>" alt="<%=Model.Logo.Alt%>"/>
  <% } %>
  <%=Html.Encode(Model.Name)%></h1>
<p><i><%=Html.Encode(Model.Slogan)%></i></p>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (ProductCategory pc in Model.Categories ) { %>
<div>
 	<h2><%= Html.TranslatedActionLink( pc.Name, "ProductCategory", new { id = Model.Name, pc = pc.Reference }, new { @class="actionlink" } ) %></h2>
 	</div>

 	<% foreach (Product p in pc.Products ) { %>
<div>
 	<h3><%= Html.TranslatedActionLink( p.Name, "Product", new { id = Model.Name,  pc = pc.Reference , pref = p.Reference }, new { @class="actionlink" } ) %></h3>
 	<p>
 	<%= p.Description %>
 		<% if (p.Images !=null)
 		 foreach (ProductImage i in p.Images ) { %>
 		<img src="<%=i.Src%>" alt="<%=i.Alt%>"/>
 		<% } %>
 	</p>
</div>
<% } %>
<% } %>

	
</asp:Content>
