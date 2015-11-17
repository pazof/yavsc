yavsc
=====

[doc-fr](http://yavsc.pschneider.fr/Blogs/UserPost/paul/Documentation)

# TODO FIRST

1) Implement a Skills provider

2) Create an `UserCardControl`
  with quick access for users to his chat and the circle membership, and for admins to his roles, a blogentry count, and a link to the booking system

3) Api refatoring:

  Concerning the blog entry edition, we only need Two methods: 

  * ```long PostFile(long id)```, 
    used for creation when the given id is 0, in which case, the entry id created is returned.
    Otherwise, used for import in the post spécified by its id, in which case, 0 is returned.
  * `long Post(BlogEntry be)`, used to create or update a given or not 
    blog entry content. the returned value is the entry id at creation, or 0.

4) UI themes
