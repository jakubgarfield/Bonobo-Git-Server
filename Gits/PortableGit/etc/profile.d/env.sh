# Add bin path in the home directory ontop of the PATH variable
export PATH="$HOME/bin:$PATH"

# Allow SSH to ask via GUI if the terminal is not usable
test -n "$SSH_ASKPASS" || {
	case "$MSYSTEM" in
	MINGW64)
		export DISPLAY=needs-to-be-defined
		export SSH_ASKPASS=/mingw64/libexec/git-core/git-gui--askpass
		;;
	MINGW32)
		export DISPLAY=needs-to-be-defined
		export SSH_ASKPASS=/mingw32/libexec/git-core/git-gui--askpass
		;;
	esac
}
