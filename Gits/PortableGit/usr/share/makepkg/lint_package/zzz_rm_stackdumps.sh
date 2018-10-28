#!/usr/bin/bash
#
#   zzz_rm_stackdumps.sh - Remove .stackdump files
#

[[ -n "$LIBMAKEPKG_LINT_PACKAGE_RM_STACKDUMPS_SH" ]] && return
LIBMAKEPKG_LINT_PACKAGE_RM_STACKDUMPS_SH=1

lint_package_functions+=('zzz_rm_stackdumps')

zzz_rm_stackdumps() {
	find "${pkgdir}" -type f -name \*.stackdump -exec rm -v {} \;
}
