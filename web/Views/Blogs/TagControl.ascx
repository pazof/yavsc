<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BasePost>" %>
<ul id="tags<%=Model.Id%>" data-postid="<%=Model.Id%>" class="editablelist ">
<% if (Model.Tags != null) {
foreach ( var tagname in Model.Tags) { %>
<li><i class="fa fa-tag"></i><%=tagname%></li><%
%><% } } %>
</ul>
<% if (Membership.GetUser()!=null) { %>
<% if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
{ // grant all permissions: to choose a given set of tags, also create some new tags %>

<form id="tagger<%=Model.Id%>">
<fieldset>
<legend><i class="fa fa-tags"></i> <%=Html.Translate("DoTag")%></legend>
<div>
 <label for="newtag"><%= Html.Translate("Tag_name")%>: </label>
 <span id="Err_tag<%=Model.Id%>" class="error"></span>
<input type="text" id="newtag<%=Model.Id%>" class="taginput">
<span id="Err_model<%=Model.Id%>" class="error"></span>
<input id="sendnewtag<%=Model.Id%>" type="submit" class="link fa fa-tag" value="<%=Html.Translate("Submit")%>">
</div>
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
