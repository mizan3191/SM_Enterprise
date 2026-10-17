--AES-256-CBC দিয়ে একটা license token বানিয়ে দাও।

--Key (Base64): tdTotvpj5cDxKVXZtL84RutnWtzyJFYAleSbMHHPrNc=
--IV (Base64): FxkvwzGCNFrMVx9CXSzkAw==
--Mode: CBC, Padding: PKCS7 (C# Aes.Create() এর default)
--Plaintext format: "YYYY-MM-DD|DEALER"
--Output: ciphertext এর Base64 string

--তারিখ: 2026-10-31
--Plaintext হবে: "2026-12-01|DEALER"

--শর্ত:
--1. অবশ্যই code চালিয়ে (C#) encrypt করবে, নিজে আন্দাজে লিখবে না।
--2. Encrypt করার পর আবার decrypt করে দেখাবে যে plaintext ঠিক "2026-12-01|DEALER" আসছে।
--3. শেষে শুধু এই JSON অংশটা দেবে:
--   "ExpiryToken": "<token>"