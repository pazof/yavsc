<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
<ul id="tags<%=Model.Id%>" data-postid="<%=Model.Id%>" class="editablelist ">
<% if (Model.Tags != null) {
foreach ( var tagname in Model.Tags) { %>
<li class="tagname fa fa-tag"><%=tagname%></li> <%
%><% } } %>
</ul>
<% if (Membership.GetUser()!=null) { %>
<% if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
{ // grant all permissions: to choose a given set of tags, also create some new tags %>

<span id="viewtagger<%=Model.Id%>">
<i class="fa fa-tag menuitem" id="viewtaggerbtn<%=Model.Id%>"><%=Html.Translate("DoTag")%></i></span>
<span id="hidetagger<%=Model.Id%>" class="hidden">
<i class="fa fa-tag menuitem"  id="hidetaggerbtn<%=Model.Id%>" ><%=Html.Translate("Tags")%> - <%=Html.Translate("Hide")%></i>
Note: Ils sont utilisé pour classifier le document. Par exemple, le tag <code>Accueil</code> rend le document 
éligible à une place en page d'Accueil.
</span>
<form id="tagger<%=Model.Id%>" class="maskable" data-btn-show="viewtagger<%=Model.Id%>" data-btn-hide="hidetagger<%=Model.Id%>" >
<fieldset>
<legend>Associer des tags au billet</legend>
 <label for="newtag"><%= Html.Translate("Tag_name")%>: </label>
 <span id="Err_tag<%=Model.Id%>" class="error"></span>
<input type="text" id="newtag<%=Model.Id%>" class="taginput">
<span id="Err_model<%=Model.Id%>" class="error"></span>
<input id="sendnewtag<%=Model.Id%>" type="submit" class="submittag fa fa-tag" value="<%=Html.Translate("Submit")%>">
</fieldset>
</form>
<script>
	$('#sendnewtag<%=Model.Id%>').click(function(e){
		Tags.tag(<%=Model.Id%>,$('#newtag<%=Model.Id%>').val(),
		function(postid,tagname){
			var nTagDisplay = $('<span class="tagname">'+tagname+'</span>');
			nTagDisplay.click( function() {
	   var postid=$(this).parent().data('postid');
	   var $thistag = $(this);
	   Tags.untag(postid, $thistag.text(), function () { $thistag.remove(); } );
	   });
            nTagDisplay.appendTo('#tags'+postid);
            $('#newtag'+postid).val('');
            }
		);
		e.preventDefault();
	});
	$('#sendnewtag<%=Model.Id%>').on('submit', function(e) { e.preventDefault(); });
</script>
<% } %>
<% } %>
