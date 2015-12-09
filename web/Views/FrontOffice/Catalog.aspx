<%@ Page Title="Catalog" Language="C#" Inherits="System.Web.Mvc.ViewPage<Catalog>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% foreach (Brand b in Model.Brands ) { %><div>
 	 <h1>	<%= Html.TranslatedActionLink( b.Name, "Brand", new { id = b.Name }, new { @class="actionlink" } ) %> </h1>
 	 <p><i><%= Html.Encode( b.Slogan ) %></i></p>
 	<% foreach (ProductCategory pc in b.Categories ) { %>
<div>
 	<h2><%= Html.TranslatedActionLink( pc.Name, "ProductCategory", new { brandid= b.Name, pcid = pc.Reference }, new { @class="actionlink" } ) %></h2>
 	</div>

 	<% foreach (Product p in pc.Products ) { %>
<div>
 	<h3><%= Html.TranslatedActionLink( p.Name, "Product", 
 	new { id = b.Name,  pc = pc.Reference , pref = p.Reference }, 
 	new { @class="actionlink" } ) %></h3>
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
</div><% } %>
</asp:Content>


