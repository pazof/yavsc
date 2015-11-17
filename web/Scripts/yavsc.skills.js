
var Skills =  (function(){ 
var self = {};
self.updateSkill = function () { } ;

self.createSkill = function (skillname,callback) {
var skill = { name: skillname } ;
 console.log('creating the skill : '+skill.name+' ... ');
Yavsc.ajax('Skill/DeclareSkill',skill,callback);
};
self.deleteSkill = function (sid,callback) {
Yavsc.ajax('Skill/DeleteSkill',sid,callback);
};
self.declareUserSkill = function (usersdec,callback) {
 console.log("creating the user's skill [ "+usersdec.skillid +', '+usersdec.comment+' ] ... ');
Yavsc.ajax('Skill/DeclareUserSkill',usersdec,callback);
};

return self;
})();
