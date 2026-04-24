package com.mycompany.myshop.testkit.common;

public class Closer {
    private Closer() {
        throw new IllegalStateException("Utility class");
    }

    public static void close(AutoCloseable closeable) {
        if (closeable != null) {
            try {
                closeable.close();
            } catch (Exception e) {
                throw new IllegalStateException("Failed to close resource", e);
            }
        }
    }
}


