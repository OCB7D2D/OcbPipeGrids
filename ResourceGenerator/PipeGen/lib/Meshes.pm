use strict;
use warnings;
package Meshes;
use base qw(Mesh);
use Math::Vector::Real;

sub Face
{
	die "Inefficient";
}

sub Faces
{
	map { $_->Faces } @{$_[0]->{meshes}}
}

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new();
	$self->{meshes} = [];
	$self->AddMesh($_) foreach (@_);
	return $self;
}

# Add vertices of a complex object
# Act a bit like selection of vertices
sub AddMesh
{
	my ($self, $mesh) = @_;
	push @{$self->{meshes}}, $mesh;
	return $self;
}

sub clone
{
	my $self = shift;
	return bless {
		meshes => [
			map { $_->clone }
			@{$self->{meshes}}
		]
	}, ref $self;
}

1;