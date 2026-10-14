# React File Guide

Source files contain learning headers. The remaining project files have these roles:

- `package.json`: valid JSON manifest defining React/Vite dependencies and npm commands. JSON comments would break npm parsing.
- `package-lock.json`: generated dependency lock file; never edit manually.
- `tsconfig*.json`: TypeScript compiler settings.
- `vite.config.ts`: Vite configuration; it has an inline header because TypeScript supports comments.
- `index.html`: minimal HTML host page into which `main.tsx` mounts React.
- `src/features/products/ProductList.tsx`: product catalog and quantity UI.
- `src/features/customers/CustomerForm.tsx`: customer registration UI.
- `src/features/orders/OrderHistory.tsx`: session order display.
- `src/shared/api.ts`: shared HTTP request helper and API base URL.
- `src/shared/models.ts`: shared TypeScript API response contracts.
- `public/` and `src/assets/`: static browser assets. The generated images are not required by the custom Phase 2 screen.
- `node_modules/` and `dist/`: downloaded/build-generated output; do not edit or document file-by-file.
