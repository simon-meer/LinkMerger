# Link Merger
Small utility package that merges all `link.xml` files in dependencies at build time.

When building the project, this utility will:

1. Look for all dependencies that have a `link.xml` in their **root** directory.
2. Merge those files into the `link.xml` located at the project's assets root truncating everything below the `<--AUTO-GENERATED-->` mark.