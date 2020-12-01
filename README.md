# Link Merger
Small utility package that merges all `link.xml` files in dependencies at build time.

When building the project, this utility will:

1. Look for all dependencies that have a `link.xml` in their **root** directory.
2. Merge those files into the `link.xml` located at the project's assets root truncating everything below the `<--AUTO-GENERATED-->` mark.


# Install

Simply add this repository to your project either by using the package manager, or by adding the following to your `manifest.json`:

`"ch.zomg.link-merger": "https://github.com/simon-meer/link-merger.git"`
