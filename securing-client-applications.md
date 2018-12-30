# Concepts

NOTE: THIS DOCUMENT IS IN PROGRESS AND HIGHLY THEORETICAL

Application security in a environment where your application (or users) can access arbitrary data and scripts is significantly different than in environments where a developer has absolute control on the files users have access to.

As a developer, if you load ANY user content dynamically you must load it in such a way that it can not be executed as a script

## Hashed Content

Linking to content or scripts you create using hash link can be considered safe, but only if you verify the hash.

Many browsers can hash references:
<script src="somthing.js" integrity="sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa"></script>

However, there is no easy way to verify the initial HTML file since hashing would happen after load, and is susceptible to man-in-the-middle attacks.

Ideally, Any initial content (entry pages) you serve used to verify hashes should be served from a node that you control or a node you trust

A possible solution is:

* DNS round-robin points to all nodes you control, or nodes you trust (Use DNSSEC if possible)
* These nodes may use a trusted or self signed cert depending on need
* Entry url should serve bootstrapping html/scripts that can load and hash content securely

Potentially A browser plugin could also solve this issue by adding a way to hash a bootstrapping script and hash it at the same time.

## Signed Content

If you don't care about the specific hash of a item, you can always check that the signature is one that you trust and the hash is as described. Signing should be the only way you verify the author of any given content.

## Same-origin

Same-origin is a method of blocking scripts loaded from another domain 

https://developer.mozilla.org/en-US/docs/Web/Security/Same-origin_policy

## Mime-types

Some protection may also be gained from always using the proper mime-types