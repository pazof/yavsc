
var Skills =  (function(){ 
var self = {};
self.updateSkill = function () { } ;

self.createSkill = function (skill,callback) {
 console.log('creating the skill : '+skill.name+','+skill.meacode+' ... ');
Yavsc.ajax('Skill/DeclareSkill',skill,callback);
};
self.deleteSkill = function (sid,callback) {
Yavsc.ajax('Skill/DeleteSkill',sid,callback);
};
self.declareUserSkill = function (usersdec,callback) {
Yavsc.ajax('Skill/DeclareUserSkill',usersdec,callback);
};

return self;
})();
