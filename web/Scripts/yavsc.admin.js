
var Admin =  (function(){ 
var self = {};
self.addUserToRole = function  (user,role,callback) {
	Yavsc.ajax('Account/AddUserToRole', 
		{ username: user, role: role }, callback );
};
self.removeUserFromRole = function (user,role,callback) {
		Yavsc.ajax('Account/RemoveUserFromRole', 
		{ username: user, role: role }, callback );
		}

return self;
})();

(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.decharger', {
     options: {
     	roleName: 'Admin',
	 },
     _create: function() {
     var _this = this;
	 var $bgobj = $(this.element); 
	 // console.log ('Dechargeable:'+$bgobj.text());
	  $bgobj.click(function() {
	   var user = $bgobj.text();
	   Admin.removeUserFromRole(user,_this.options.roleName,
	    function() {
	    $bgobj.remove();
	    });
	  });
	  },
});
})(jQuery);
}).call(this);

