# 049 - Commissions

**Status:** Implemented and verified locally; deployment pending

## Feature Description

Add a dedicated Commissions gallery for projects the owner plans to sell. Like
the blog, the gallery will have a listing and individual pages managed through
the site, but each entry will describe a product and invite visitors to inquire
by email. Place Commissions between Home and Blog in the main menu.

## Goals

- Let visitors browse products and open a dedicated page for each project.
- Show a slideshow of project images, a description, customization options with
  optional individual prices, and a clearly identified base asking price on
  each product page.
- Provide an email link for visitors to ask for more information.
- Let authorized users create, edit, publish, and remove gallery entries without
  changing site code.
- Support owner-directed product and image management through the existing MCP
  server, with publication controlled in the admin UI.
- Respect the visitor's selected site theme throughout the Commissions UI.

## Technical Approach

- Create a separate product content model with a title, URL-friendly identifier,
  description, customization options with optional prices, a base price in US
  dollars, a published/unpublished status, and an ordered collection of images
  with alternative text.
- Store products and their images in PostgreSQL. Reuse suitable patterns from
  blog post authoring and image management without mixing products into the
  public blog feed.
- Add an admin workflow for product details, image upload and ordering, and
  publication management. Apply server-side authorization and ensure
  unpublished product details and images are not publicly accessible.
- Add a public gallery listing and product detail pages. The slideshow should
  work with keyboard and touch, expose accessible controls and image
  descriptions, and handle one image or no images gracefully.
- Use the existing theme system for the gallery, product pages, slideshow
  controls, and admin UI, including readable text and visible focus states.
- Display the asking price as a starting price and show an option's price when
  provided; an option without a price remains visible without a price. Add a
  `mailto:` link to the shared address `fortinbra@becauseimclever.com`,
  identifying the product in the subject where practical.
- Extend the existing MCP endpoint with tools to list and read products and to
  create, update, and delete unpublished products. Include uploading, updating,
  reordering, and deleting images on unpublished products.
- Follow the existing MCP bearer authentication, bounded inputs, explicit tool
  contracts, and concurrency safeguards. Atomically reject changes if a
  product is published or changed concurrently; keep published products and
  their images read-only through MCP. Publication and unpublication remain
  admin UI operations.
- Cover product management, publication and image access, price display,
  slideshow and contact behavior, and MCP authorization and mutation boundaries
  with focused tests.

## Affected Components/Layers

- **Domain and Application:** Product and image data, validation, and
  management/query contracts.
- **Infrastructure:** PostgreSQL persistence, migration, and image storage.
- **Server:** Public read and authorized management endpoints, following
  existing API versioning and access-control conventions; extend the existing
  MCP tools for products and images.
- **Client:** Gallery listing, product page and slideshow, admin editing, and
  navigation.

## Design Decisions

- Keep the gallery distinct from blog posts and the existing GitHub-based
  Projects page; the new pages serve a different purpose and should not
  silently replace existing content.
- Treat the displayed price as a base asking price, not a final quote. Visitors
  contact the owner directly to discuss customizations and details.
- Use US dollars for both the base price and any customization prices.
- Products are either published or unpublished; scheduling is not required.
- Match the blog MCP workflow: the owner can manage unpublished products and
  their images, but published products are read-only and publication changes
  require the admin UI.
- No shopping cart, payment processing, inventory, order management, or
  automatic calculation of a final customized price is included in this feature.

## Acceptance Criteria

- An authorized user can create and edit a product, manage its images and
  ordering, set optional customization prices, publish or unpublish it, and
  remove it.
- Published products appear in the gallery and have directly accessible detail
  pages showing the description, customization options and any provided prices,
  base price in US dollars, image slideshow, and shared email contact link.
- Commissions appears between Home and Blog in the main menu.
- Unpublished products and their images are unavailable to anonymous visitors.
- An authenticated MCP client can list and read products, manage unpublished
  product details and images, and cannot publish, unpublish, or modify published
  products or their images. Concurrent publication must prevent draft changes.
- The gallery and slideshow remain usable on mobile and with a keyboard;
  images have meaningful alternative text.
- The Commissions listing, detail pages, slideshow controls, and admin UI
  reflect the selected theme and remain readable and usable when it changes.
- Existing blog posts and the GitHub Projects page continue to work.

## Implementation Notes

- Public routes are `/commissions` and `/commissions/{slug}`; administrator
  management is under `/admin/commissions`, restricted to the existing Admin
  policy. Public and admin APIs use `/api/v1/commissions` and
  `/api/v1/admin/commissions`.
- Descriptions are plain text. All prices are USD, from zero to $999,999.99 with
  at most two decimal places. An omitted customization price is not treated as
  zero.
- Supported images are PNG, JPEG, GIF, and WebP, limited to 5 MiB each and 20 per
  product, with required alternative text. The first image is the cover.
- The slideshow uses manual previous/next controls, arrow keys, and horizontal
  touch swipes, with no automatic motion. Shared theme variables and button
  styles apply to public and admin UI, including focus states.
- PostgreSQL stores product fields, customization JSON, and image bytes.
  Migration `AddCommissions` adds the tables without changing blog data.
  Every mutation advances a product-level concurrency version. EF transactions
  roll back image mutations if a concurrent edit or publication wins.
- Nine commission MCP tools extend the existing endpoint and require the same
  private bearer configuration. Their restricted service cannot publish,
  unpublish, or modify published entries. Image uploads use base64 in a bounded
  request body; the MCP body limit is now 8 MiB.
- The shared inquiry address is not editable per product. No new deployment
  service or commerce provider is required.

## Verification

- Solution build and Release publish succeeded with no warnings or errors.
- 158 focused and regression checks passed: 42 client, 19 infrastructure, and
  97 server tests. These cover optional and zero prices, safe description
  rendering, slideshow buttons/keyboard/swipes, editor conflict and unsaved-work
  protection, API authorization and visibility, and restricted MCP behavior.
- Real disposable PostgreSQL tests verified migrations, JSON customization
  persistence, image byte preservation, cascade deletion, and rollback of an
  image upload racing with publication. Image limit tests cover exactly 5 MiB
  and rejection above that size. CI reuses its existing PostgreSQL service for
  the database tests.
- Browser verification of the published build confirmed gallery navigation,
  prices and email links, loaded images, slideshow navigation, and unpublished
  visibility. All 11 themes updated slideshow styling; theme selection
  persisted across reload. Listing and detail pages fit a 390-pixel viewport
  without horizontal overflow.
- Generated OpenAPI includes all nine commission route patterns.
- No production deployment or production database changes were performed.
